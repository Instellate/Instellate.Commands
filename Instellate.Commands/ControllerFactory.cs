using System.Reflection;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Instellate.Commands.Actions;
using Instellate.Commands.ArgumentParser;
using Instellate.Commands.Attributes;
using Instellate.Commands.Attributes.Text;
using Instellate.Commands.Commands;
using Instellate.Commands.Converters;
using Instellate.Commands.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Instellate.Commands;

public class ControllerFactory
{
    private readonly Dictionary<string, ICommand> _commands = [];
    private readonly Dictionary<string, ContextMenu> _contextMenus = [];
    private readonly ILogger<ControllerFactory> _logger;
    private readonly IServiceProvider _provider;

    public IReadOnlyDictionary<string, ICommand> Commands => this._commands;
    public IReadOnlyDictionary<string, ContextMenu> ContextMenus => this._contextMenus;

    public ControllerFactory(
        ILogger<ControllerFactory> logger,
        IServiceProvider provider
    )
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

            if (!type.IsSubclassOf(typeof(BaseController)))
            {
                this._logger.LogWarning(
                    "Class {ClassName} is marked with BaseController but does not derive the BaseController class. This class will not be registerd",
                    type.Name
                );
            }
            else
            {
                controllerTypes.Add(type);
            }
        }

        foreach (Type type in controllerTypes)
        {
            CommandAttribute? groupAttr = type.GetCustomAttribute<CommandAttribute>();
            CommandGroup? group = null;
            if (groupAttr is not null)
            {
                group = new CommandGroup(
                    groupAttr.Name,
                    groupAttr.Description,
                    type
                );
            }

            foreach (MethodInfo method in type.GetMethods())
            {
                CommandAttribute? commandAttr = method.GetCustomAttribute<CommandAttribute>();
                ContextMenuAttribute? contextMenuAttr
                    = method.GetCustomAttribute<ContextMenuAttribute>();

                if (commandAttr is not null)
                {
                    AddCommand(commandAttr, group, method);
                }
                else if (contextMenuAttr is not null)
                {
                    ContextMenu contextMenu = new(contextMenuAttr.Name, method);
                    this._contextMenus.Add(contextMenu.Name, contextMenu);
                    this._logger.LogTrace("Mapped context menu {ContextMenu}", contextMenu.Name);
                }
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
        List<DiscordApplicationCommand> commands
            = new(this._commands.Count + this._contextMenus.Count);

        foreach (ICommand command in this._commands.Values)
        {
            commands.Add(command.ConstructApplicationCommand());
        }

        foreach (ContextMenu contextMenu in this._contextMenus.Values)
        {
            commands.Add(contextMenu.ConstructApplicationCommand());
        }

        if (debugGuildId is { } guildId)
        {
            return client.BulkOverwriteGuildApplicationCommandsAsync(guildId, commands);
        }

        return client.BulkOverwriteGlobalApplicationCommandsAsync(commands);
    }

    internal async Task HandleMessageCreatedAsync(DiscordClient client, MessageCreatedEventArgs e)
    {
        string content = e.Message.Content;
        if (string.IsNullOrWhiteSpace(content))
        {
            return;
        }

        using IServiceScope scope = this._provider.CreateScope();

        MessageActionContext actionContext = new(e, client);
        IPrefixResolver resolver = scope.ServiceProvider.GetRequiredService<IPrefixResolver>();
        string? prefix = await resolver.ResolvePrefixAsync(actionContext);
        if (prefix is null)
        {
            return;
        }

        if (!content.StartsWith(prefix))
        {
            return;
        }

        IErrorHandler? errorHandler = scope.ServiceProvider.GetService<IErrorHandler>();

        content = content[prefix.Length..];
        Parser.Result result;
        try
        {
            result = Parser.Parse(content);
        }
        catch (CommandsException)
        {
            return;
        }

        ICommand? command = null;
        int i = 0;
        for (; i < result.Commands.Count; i++)
        {
            string commandName = result.Commands[i];

            if (command is CommandGroup group)
            {
                if (group.RequirePermissions is not null &&
                    !await MemberHasPermissionAsync(e, group.RequirePermissions.Value))
                {
                    if (errorHandler is not null)
                    {
                        IActionResult actionResult
                            = await errorHandler.HandleAuthorMissingPermissionsAsync(
                                command,
                                group.RequirePermissions.Value
                            );
                        await actionResult.ExecuteResultAsync(actionContext);
                    }

                    return;
                }

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

        if (executor.RequirePermissions is not null &&
            !await MemberHasPermissionAsync(e, executor.RequirePermissions.Value))
        {
            if (errorHandler is not null)
            {
                IActionResult actionResult
                    = await errorHandler.HandleAuthorMissingPermissionsAsync(
                        command,
                        executor.RequirePermissions.Value
                    );
                await actionResult.ExecuteResultAsync(actionContext);
            }
        }

        if (i < result.Commands.Count)
        {
            List<string> remainingPositional =
                result.Commands.GetRange(i, result.Commands.Count - i);
            result.PositionalArguments.InsertRange(0, remainingPositional);
        }

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
                        options.Add(option.DefaultValue);
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
                        options.Add(option.DefaultValue);
                        continue;
                    }

                    strOption = new Converters.Optional<string>(false);
                }
            }

            try
            {
                IConverter converter =
                    (IConverter)scope.ServiceProvider.GetRequiredService(option.ConverterType);
                options.Add(await converter.ConvertFromString(strOption, actionContext));
            }
            catch (Exception exception)
            {
                this._logger.LogError(exception, "Got error when trying to convert value");
                if (errorHandler is not null)
                {
                    IActionResult actionResult = await errorHandler.HandleConverterErrorAsync(
                        executor,
                        option,
                        strOption,
                        exception
                    );
                    await actionResult.ExecuteResultAsync(actionContext);
                }

                return;
            }
        }

        try
        {
            await ExecuteCommandAsync(
                executor,
                options,
                scope.ServiceProvider,
                actionContext,
                client,
                e.Author,
                e.Channel,
                e.Message
            );
        }
        catch (Exception exception)
        {
            this._logger.LogError(exception, "Got error when trying to execute command");
            if (errorHandler is not null)
            {
                IActionResult actionResult =
                    await errorHandler.HandleCommandErrorAsync(executor, exception);
                await actionResult.ExecuteResultAsync(actionContext);
            }
        }
    }

    internal async Task HandleInteractionCreatedAsync(
        DiscordClient client,
        InteractionCreatedEventArgs e
    )
    {
        if (e.Interaction.Type != DiscordInteractionType.ApplicationCommand)
        {
            return;
        }

        DiscordInteraction interaction = e.Interaction;
        if (interaction.Data.Type == DiscordApplicationCommandType.MessageContextMenu ||
            interaction.Data.Type == DiscordApplicationCommandType.UserContextMenu)
        {
            await HandleContextMenuAsync(client, interaction);
            return;
        }

        using IServiceScope scope = this._provider.CreateScope();
        IErrorHandler? errorHandler = scope.ServiceProvider.GetService<IErrorHandler>();

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
                        commandGroup.Name
                    );
                    return;
                }

                if (!commandGroup.Children.TryGetValue(name, out command))
                {
                    this._logger.LogWarning(
                        "Couldn't get command with name {Name} for command group {CommandGroup}",
                        name,
                        commandGroup.Name
                    );
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

        InteractionActionContext actionContext = new(interaction, client);
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
                    options.Add(option.DefaultValue);
                    continue;
                }

                this._logger.LogError(
                    "Couldn't find option {Option} for commmand {Command}",
                    option.Name,
                    executor.Name
                );
                if (errorHandler is not null)
                {
                    CommandsException exception = new($"Option {option.Name} not found");
                    IActionResult actionResult
                        = await errorHandler.HandleCommandErrorAsync(executor, exception);
                    await actionResult.ExecuteResultAsync(actionContext);
                }

                return;
            }

            try
            {
                IConverter converter =
                    (IConverter)scope.ServiceProvider.GetRequiredService(option.ConverterType);
                options.Add(await converter.ConvertFromObject(dataOption.Value, actionContext));
            }
            catch (Exception exception)
            {
                this._logger.LogError(exception, "Got error when trying to convert value");
                if (errorHandler is not null)
                {
                    IActionResult actionResult = await errorHandler.HandleConverterErrorAsync(
                        executor,
                        option,
                        dataOption.Value,
                        exception
                    );
                    await actionResult.ExecuteResultAsync(actionContext);
                }

                return;
            }
        }

        try
        {
            await ExecuteCommandAsync(
                executor,
                options,
                scope.ServiceProvider,
                actionContext,
                client,
                interaction.User,
                interaction.Channel,
                interaction.Message
            );
        }
        catch (Exception exception)
        {
            this._logger.LogError(exception, "Got error when trying to execute command");
            if (errorHandler is not null)
            {
                IActionResult actionResult =
                    await errorHandler.HandleCommandErrorAsync(executor, exception);
                await actionResult.ExecuteResultAsync(actionContext);
            }
        }
    }

    private async Task HandleContextMenuAsync(DiscordClient client, DiscordInteraction interaction)
    {
        using IServiceScope scope = this._provider.CreateScope();

        if (!this._contextMenus.TryGetValue(interaction.Data.Name, out ContextMenu? menu))
        {
            this._logger.LogWarning("Couldn't find context menu {Name}", interaction.Data.Name);
            return;
        }

        InteractionActionContext actionContext = new(interaction, client);
        BaseController controller = PrepareController(
            menu._controllerType,
            scope.ServiceProvider,
            actionContext,
            client,
            interaction.User,
            interaction.Channel,
            interaction.Message
        );

        IActionResult actionResult = await menu._executionLambda(controller);
        await actionResult.ExecuteResultAsync(controller.ActionContext);
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
                throw new CommandsException(
                    $"Paremeter needs to specify a {nameof(OptionAttribute)} specified"
                );
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

            CommandOptionMetadata metadata = new(
                option.Name,
                option.Description,
                optional,
                positional
            )
            {
                MinValue = parameter.GetCustomAttribute<MinValueAttribute>()?.Value,
                MaxValue = parameter.GetCustomAttribute<MaxValueAttribute>()?.Value
            };

            Type converterType = typeof(IConverter<>).MakeGenericType(parameter.ParameterType);

            IConverter converter = (IConverter)this._provider.GetRequiredService(converterType);
            CommandOption commandOption = converter.ConstructOption(metadata);
            commandOption.ConverterType = converterType;
            commandOption.ParameterType = parameter.ParameterType;
            commandOption.DefaultValue = parameter.DefaultValue;
            options.Add(commandOption);
        }

        return options;
    }

    private BaseController PrepareController(
        Type type,
        IServiceProvider provider,
        IActionContext actionContext,
        DiscordClient client,
        DiscordUser author,
        DiscordChannel channel,
        DiscordMessage? message
    )
    {
        BaseController controller =
            (BaseController)provider.GetRequiredService(type);
        controller.ActionContext = actionContext;
        controller.Client = client;
        controller.Author = author;
        controller.Channel = channel;
        controller.Message = message;

        return controller;
    }

    private async Task ExecuteCommandAsync(
        Command command,
        IReadOnlyList<object?> options,
        IServiceProvider provider,
        IActionContext actionContext,
        DiscordClient client,
        DiscordUser author,
        DiscordChannel channel,
        DiscordMessage? message
    )
    {
        BaseController controller = PrepareController(
            command._method.DeclaringType!,
            provider,
            actionContext,
            client,
            author,
            channel,
            message
        );

        IActionResult result = await command._executionLambda(controller, options);
        await result.ExecuteResultAsync(controller.ActionContext);
    }

    private void AddCommand(CommandAttribute command, CommandGroup? group, MethodInfo method)
    {
        IReadOnlyList<CommandOption> options = GetOptionsForMethod(method);
        Command commandValue = new(
            command.Name,
            command.Description,
            options,
            method.GetCustomAttribute<RequirePermissionsAttribute>()?.Permissions,
            method
        );

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

    private async Task<bool> MemberHasPermissionAsync(
        MessageCreatedEventArgs e,
        DiscordPermissions requiredPermissions
    )
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (e.Guild is null)
        {
            return true;
        }

        DiscordMember member = await e.Guild.GetMemberAsync(e.Author.Id);
        return member.Permissions.HasAllPermissions(requiredPermissions);
    }
}
