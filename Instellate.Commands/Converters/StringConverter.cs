using DSharpPlus.Entities;
using Instellate.Commands.Actions;
using Instellate.Commands.Commands;

namespace Instellate.Commands.Converters;

public class StringConverter : IConverter<string>
{
    public DiscordApplicationCommandOptionType Type => DiscordApplicationCommandOptionType.String;

    public CommandOption ConstructOption(CommandOptionMetadata metadata)
    {
        return new CommandOption(
            metadata.Name,
            metadata.Description,
            metadata.Optional,
            metadata.Positional
        )
        {
            Type = this.Type
        };
    }

    public ValueTask<object?> ConvertFromObject(object? obj, IActionContext context)
    {
        if (obj is not string)
        {
            throw new ArgumentException("Object is not string", nameof(obj));
        }

        return ValueTask.FromResult<object?>(obj);
    }

    public ValueTask<object?> ConvertFromString(Optional<string> input, IActionContext context)
    {
        if (input.TryGetValue(out string? value))
        {
            return ValueTask.FromResult<object?>(value);
        }

        throw new ArgumentNullException(nameof(input));
    }
}
