using System.Reflection;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Instellate.Commands.Actions;
using Instellate.Commands.ArgumentParser;
using Instellate.Commands.Attributes;
using Instellate.Commands.Attributes.Text;
using Instellate.Commands.Commands;
using Instellate.Commands.Commands.Text;
using Instellate.Commands.Controllers;
using Instellate.Commands.Converters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Instellate.Commands;

public class ControllerFactory
{
    private readonly ILogger<ControllerFactory> _logger;
    private readonly IServiceProvider _provider;
    private readonly Dictionary<string, ICommand> _commands = [];

    public ControllerFactory(ILogger<ControllerFactory> logger,
        IServiceProvider provider)
    {
        this._logger = logger;
        this._provider = provider;
    }

    public void MapControllers(Assembly assembly)
    {
        this._logger.LogTrace("Mapping controllers");

        List<Type> controllerTypes = [];
        foreach (Type type in assembly.GetExportedTypes())
        {
            if (type.GetCustomAttribute<BaseControllerAttribute>() is null)
            {
                continue;
            }

            controllerTypes.Add(type);
        }

        foreach (Type type in controllerTypes)
        {
            CommandAttribute? groupAttr = type.GetCustomAttribute<CommandAttribute>();
            CommandGroup? group = null;
            if (groupAttr is not null)
            {
                group = new CommandGroup(groupAttr.Name, groupAttr.Description);
            }

            foreach (MethodInfo method in type.GetMethods())
            {
                CommandAttribute? command = method.GetCustomAttribute<CommandAttribute>();
                if (command is null)
                {
                    continue;
                }

                IReadOnlyList<CommandOption> options = GetOptionsForMethod(method);
                Command commandValue = new(command.Name, command.Description, options, method);

                if (group is not null)
                {
                    group._children.Add(command.Name, commandValue);
                }
                else
                {
                    this._commands.Add(command.Name, commandValue);
                }

                this._logger.LogTrace("Mapped command {}", command.Name);
            }

            if (group is not null)
            {
                this._logger.LogTrace("Mapped command group {}", group.Name);
                this._commands.Add(group.Name, group);
            }
        }

        this._logger.LogTrace("Finished mapping controllers");
    }

    public Task RegisterCommandsAsync(DiscordClient client, ulong? debugGuildId)
    {
        List<DiscordApplicationCommand> commands = new(this._commands.Count);
        foreach (ICommand command in this._commands.Values)
        {
            commands.Add(command.ConstructApplicationCommand());
        }

        if (debugGuildId is { } guildId)
        {
            return client.BulkOverwriteGuildApplicationCommandsAsync(guildId, commands);
        }
        else
        {
            return client.BulkOverwriteGlobalApplicationCommandsAsync(commands);
        }
    }

    internal async Task HandleMessageCreatedAsync(DiscordClient client, MessageCreatedEventArgs e)
    {
        using IServiceScope scope = this._provider.CreateScope();

        string content = e.Message.Content;
        if (string.IsNullOrWhiteSpace(content))
        {
            return;
        }

        IPrefixResolver resolver = scope.ServiceProvider.GetRequiredService<IPrefixResolver>();
        string? prefix = await resolver.ResolvePrefixAsync(e.Message);
        if (prefix is null)
        {
            return;
        }

        if (!content.StartsWith(prefix))
        {
            return;
        }

        content = content[prefix.Length..];
        Parser.Result result = Parser.Parse(content);

        ICommand? command = null;
        int i = 0;
        for (; i < result.Commands.Count; i++)
        {
            string commandName = result.Commands[i];

            if (command is CommandGroup group)
            {
                if (group.Children.TryGetValue(commandName, out ICommand? nextCommand))
                {
                    command = nextCommand;
                }
                else
                {
                    // Command group cannot handle execution
                    return;
                }
            }
            else if (command is null)
            {
                if (this._commands.TryGetValue(commandName, out ICommand? nextCommand))
                {
                    command = nextCommand;
                }
                else
                {
                    return;
                }
            }
            else
            {
                // All other `result.Commands` values counts as positional
                break;
            }
        }

        if (command is not Command executor)
        {
            return;
        }

        if (executor.ApplicationCommandType is not null)
        {
            return;
        }

        if (i < result.Commands.Count)
        {
            List<string> remainingPositional =
                result.Commands.GetRange(i, result.Commands.Count - i);
            result.PositionalArguments.InsertRange(0, remainingPositional);
        }

        MessageActionContext actionContext = new(e);
        List<object?> options = new(executor.Options.Count);
        foreach (CommandOption option in executor.Options)
        {
            Converters.Optional<string> strOption;
            if (option.Positional is { } pos)
            {
                if (result.PositionalArguments.Count <= pos)
                {
                    if (option.Optional)
                    {
                        options.Add(null);
                        continue;
                    }

                    strOption = new Converters.Optional<string>(false);
                }
                else
                {
                    strOption = new Converters.Optional<string>(result.PositionalArguments[pos]);
                }
            }
            else
            {
                if (result.Options.TryGetValue(option.Name, out string? value))
                {
                    strOption = new Converters.Optional<string>(value, true);
                }
                else
                {
                    if (option.Optional)
                    {
                        options.Add(null);
                        continue;
                    }

                    strOption = new Converters.Optional<string>(false);
                }
            }

            try
            {
                IConverter converter =
                    (IConverter)scope.ServiceProvider.GetRequiredService(option.ConverterType);
                options.Add(converter.ConvertFromString(strOption));
            }
            catch (Exception exception)
            {
                IErrorHandler? errorHandler = scope.ServiceProvider.GetService<IErrorHandler>();
                if (errorHandler is not null)
                {
                    IActionResult actionResult = await errorHandler.HandleConverterError(executor,
                        option,
                        strOption,
                        exception);
                    await actionResult.ExecuteResultAsync(actionContext);
                }

                return;
            }
        }

        await ExecuteControllerAsync(executor,
            options.ToArray(),
            scope.ServiceProvider,
            actionContext,
            client,
            e.Author,
            e.Channel,
            e.Message);
    }

    internal async Task HandleInteractionCreatedAsync(DiscordClient client,
        InteractionCreatedEventArgs e)
    {
        using IServiceScope scope = this._provider.CreateScope();

        DiscordInteraction interaction = e.Interaction;

        string name = interaction.Data.Name;
        IReadOnlyList<DiscordInteractionDataOption> dataOptions = e.Interaction.Data.Options;

        ICommand? command = null;
        while (true)
        {
            if (command is CommandGroup commandGroup)
            {
                bool foundSubcCommand = false;
                foreach (DiscordInteractionDataOption option in dataOptions)
                {
                    if (option.Type == DiscordApplicationCommandOptionType.SubCommand ||
                        option.Type == DiscordApplicationCommandOptionType.SubCommandGroup)
                    {
                        name = option.Name;
                        dataOptions = option.Options;
                        foundSubcCommand = true;
                        break;
                    }
                }

                if (!foundSubcCommand)
                {
                    this._logger.LogWarning(
                        "Couldn't find subcommand for command group {CommandGroup}",
                        commandGroup.Name);
                    return;
                }

                if (!commandGroup.Children.TryGetValue(name, out command))
                {
                    this._logger.LogWarning(
                        "Couldn't get command with name {Name} for command group {CommandGroup}",
                        name,
                        commandGroup.Name);
                    return;
                }
            }
            else if (command is null)
            {
                if (!this._commands.TryGetValue(name, out command))
                {
                    this._logger.LogWarning("Couldn't get command with name {Name}", name);
                    return;
                }
            }
            else
            {
                break;
            }
        }

        if (command is not Command executor)
        {
            // Couldn't find command
            return;
        }

        InteractionActionContext actionContext = new(interaction);
        List<object?> options = new(executor.Options.Count);
        foreach (CommandOption option in executor.Options)
        {
            DiscordInteractionDataOption? dataOption = null;
            foreach (DiscordInteractionDataOption d in dataOptions)
            {
                if (d.Name == option.Name)
                {
                    dataOption = d;
                    break;
                }
            }

            if (dataOption is null)
            {
                if (option.Optional)
                {
                    options.Add(null);
                    continue;
                }

                throw new NotImplementedException($"Option {option.Name} not found");
            }

            try
            {
                IConverter converter =
                    (IConverter)scope.ServiceProvider.GetRequiredService(option.ConverterType);
                options.Add(converter.ConvertFromObject(dataOption.Value));
            }
            catch (Exception exception)
            {
                IErrorHandler? errorHandler = scope.ServiceProvider.GetService<IErrorHandler>();
                if (errorHandler is not null)
                {
                    IActionResult actionResult = await errorHandler.HandleConverterError(executor,
                        option,
                        dataOption.Value,
                        exception);
                    await actionResult.ExecuteResultAsync(actionContext);
                }

                return;
            }
        }

        
        try
        {
            await ExecuteControllerAsync(executor,
                options.ToArray(),
                scope.ServiceProvider,
                actionContext,
                client,
                interaction.User,
                interaction.Channel,
                interaction.Message);
        }
        catch (Exception exception)
        {
            IErrorHandler? errorHandler = scope.ServiceProvider.GetService<IErrorHandler>();
            if (errorHandler is not null)
            {
                IActionResult actionResult =
                    await errorHandler.HandleCommandError(executor, exception);
                await actionResult.ExecuteResultAsync(actionContext);
            }
        }
    }

    private IReadOnlyList<CommandOption> GetOptionsForMethod(MethodInfo method)
    {
        List<CommandOption> options = [];
        NullabilityInfoContext nullabilityInfoContext = new();

        foreach (ParameterInfo parameter in method.GetParameters())
        {
            OptionAttribute? option = parameter.GetCustomAttribute<OptionAttribute>();
            if (option is null)
            {
                throw new NotImplementedException("Proper error message not implemented");
            }

            int? positional = null;
            PositionalAttribute? positionalAttribute =
                parameter.GetCustomAttribute<PositionalAttribute>();
            if (positionalAttribute is not null)
            {
                positional = positionalAttribute.Index;
            }

            bool optional = nullabilityInfoContext.Create(parameter).ReadState ==
                NullabilityState.Nullable || parameter.HasDefaultValue;

            CommandOptionMetadata metadata = new(option.Name,
                option.Description,
                optional,
                positional)
            {
                MinValue = parameter.GetCustomAttribute<MinValueAttribute>()?.Value,
                MaxValue = parameter.GetCustomAttribute<MaxValueAttribute>()?.Value
            };

            Type converterType = typeof(IConverter<>).MakeGenericType(parameter.ParameterType);

            IConverter converter = (IConverter)this._provider.GetRequiredService(converterType);
            CommandOption commandOption = converter.ConstructOption(metadata);
            commandOption.ConverterType = converterType;
            options.Add(commandOption);
        }

        return options;
    }

    private async Task ExecuteControllerAsync(Command command,
        object?[]? options,
        IServiceProvider provider,
        IActionContext actionContext,
        DiscordClient client,
        DiscordUser author,
        DiscordChannel channel,
        DiscordMessage? message)
    {
        BaseController controller =
            (BaseController)provider.GetRequiredService(command.Method
                .DeclaringType!);

        controller.ActionContext = actionContext;
        controller.Client = client;
        controller.Author = author;
        controller.Channel = channel;
        controller.Message = message;

        object? methodResult = command.Method.Invoke(controller, options);
        if (methodResult is IActionResult actionResult)
        {
            await actionResult.ExecuteResultAsync(actionContext);
        }
        else if (methodResult is Task<IActionResult> task)
        {
            await (await task).ExecuteResultAsync(actionContext);
        }
    }
}
