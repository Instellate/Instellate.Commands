using DSharpPlus.Entities;
using Instellate.Commands.Commands;

namespace Instellate.Commands.Converters;

public class BoolConverter : IConverter<bool>
{
    public DiscordApplicationCommandOptionType Type => DiscordApplicationCommandOptionType.Boolean;


    public object ConvertFromObject(object? obj)
    {
        if (obj is not bool)
        {
            throw new ArgumentException("Object is not string", nameof(obj));
        }

        return obj;
    }

    public object ConvertFromString(Optional<string> input)
    {
        if (input.TryGetValue(out string? value))
        {
            return bool.Parse(value);
        }

        if (input.IsPresent)
        {
            return true;
        }

        return false;
    }
}
