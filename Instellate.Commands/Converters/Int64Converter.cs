using DSharpPlus.Entities;
using Instellate.Commands.Actions;
using Instellate.Commands.Commands;

namespace Instellate.Commands.Converters;

public class Int64Converter : IConverter<long>
{
    public DiscordApplicationCommandOptionType Type => DiscordApplicationCommandOptionType.Integer;

    public CommandOption ConstructOption(CommandOptionMetadata metadata)
    {
        return new CommandOption(metadata.Name,
            metadata.Description,
            metadata.Optional,
            metadata.Positional)
        {
            Type = this.Type,
            MinValue = metadata.MinValue ?? long.MinValue,
            MaxValue = metadata.MaxValue ?? long.MaxValue
        };
    }

    public ValueTask<object?> ConvertFromObject(object? obj, IActionContext context)
    {
        if (obj is not long)
        {
            throw new ArgumentException("Argument is not a long", nameof(obj));
        }

        return ValueTask.FromResult<object?>(obj);
    }

    public ValueTask<object?> ConvertFromString(Optional<string> input, IActionContext context)
    {
        if (input.TryGetValue(out string? value))
        {
            return ValueTask.FromResult<object?>(long.Parse(value));
        }

        throw new ArgumentNullException(nameof(input));
    }
}
