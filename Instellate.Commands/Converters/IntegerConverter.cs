using System.Globalization;
using System.Numerics;
using DSharpPlus.Entities;
using Instellate.Commands.Actions;
using Instellate.Commands.Commands;

namespace Instellate.Commands.Converters;

public class IntegerConverter<T> : IConverter<T> where T : struct, INumber<T>, IMinMaxValue<T>
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
            MinValue = metadata.MinValue ?? T.MinValue,
            MaxValue = metadata.MaxValue ?? T.MaxValue
        };
    }

    public ValueTask<object?> ConvertFromObject(object? obj, IActionContext context)
    {
        if (obj is not long val)
        {
            throw new ArgumentException("Argument is not a long", nameof(obj));
        }

        return ValueTask.FromResult<object?>(T.CreateChecked(val));
    }

    public ValueTask<object?> ConvertFromString(Optional<string> input, IActionContext context)
    {
        if (input.TryGetValue(out string? value))
        {
            return ValueTask.FromResult<object?>(
                T.Parse(value.AsSpan(), CultureInfo.InvariantCulture)
            );
        }

        throw new ArgumentNullException(nameof(input));
    }
}
