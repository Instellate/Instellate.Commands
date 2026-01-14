using DSharpPlus.Entities;
using Instellate.Commands.Actions;
using Instellate.Commands.Commands;

namespace Instellate.Commands.Converters;

public class Int32Converter : IConverter<int>
{
    public DiscordApplicationCommandOptionType Type => DiscordApplicationCommandOptionType.Integer;

    public CommandOption ConstructOption(CommandOptionMetadata metadata)
    {
        return new CommandOption(
            metadata.Name,
            metadata.Description,
            metadata.Optional,
            metadata.Positional
        )
        {
            Type = this.Type,
            MinValue = metadata.MinValue ?? int.MinValue,
            MaxValue = metadata.MaxValue ?? int.MaxValue
        };
    }

    public ValueTask<object?> ConvertFromObject(object? obj, IActionContext context)
    {
        if (obj is not long val)
        {
            throw new ArgumentException("Argument is not a long", nameof(obj));
        }

        return ValueTask.FromResult<object?>((int)val);
    }

    public ValueTask<object?> ConvertFromString(Optional<string> input, IActionContext context)
    {
        if (input.TryGetValue(out string? value))
        {
            return ValueTask.FromResult<object?>(int.Parse(value));
        }

        throw new ArgumentNullException(nameof(input));
    }
}
