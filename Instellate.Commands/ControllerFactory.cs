using System.Reflection;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
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

    public void RegisterDiscordEvents(EventHandlingBuilder builder)
    {
        builder.HandleMessageCreated(this.HandleMessageAsync);
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
                            NullabilityState.Nullable;

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

    private async Task HandleMessageAsync(DiscordClient client, MessageCreatedEventArgs e)
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

        if (i < result.Commands.Count)
        {
            List<string> remainingPositional =
                result.Commands.GetRange(i, result.Commands.Count - i);
            result.PositionalArguments.InsertRange(0, remainingPositional);
        }

        List<object?> options = new(executor.Options.Count);
        foreach (CommandOption option in executor.Options)
        {
            string? strOption;
            if (option.Positional is { } pos)
            {
                if (result.PositionalArguments.Count <= pos)
                {
                    if (!option.Optional)
                    {
                        // Not optional and cannot get positional
                        throw new NotImplementedException("Error not implemented");
                    }

                    options.Add(null);
                    continue;
                }
                else
                {
                    strOption = result.PositionalArguments[pos];
                }
            }
            else
            {
                if (result.Options.TryGetValue(option.Name, out string? value))
                {
                    strOption = value;
                }
                else
                {
                    if (!option.Optional)
                    {
                        throw new NotImplementedException("Error not implemented");
                    }

                    options.Add(null);
                    continue;
                }
            }

            IConverter converter =
                (IConverter)scope.ServiceProvider.GetRequiredService(option.ConverterType);
            options.Add(converter.ConvertFromString(strOption));
        }

        BaseController controller =
            (BaseController)scope.ServiceProvider.GetRequiredService(executor.Method
                .DeclaringType!);

        executor.Method.Invoke(controller, options.ToArray());
    }
}
