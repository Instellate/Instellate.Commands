using DSharpPlus.Entities;
using Instellate.Commands.Commands;

namespace Instellate.Commands.Converters;

public class Int32Converter : IConverter<int>
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
            MinValue = metadata.MinValue ?? int.MinValue,
            MaxValue = metadata.MaxValue ?? int.MaxValue
        };
    }

    public object ConvertFromObject(object obj)
    {
        if (obj is not int)
        {
            throw new ArgumentException("Argument is not a integer", nameof(obj));
        }

        return obj;
    }

    public object ConvertFromString(string? input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return int.Parse(input);
    }
}
