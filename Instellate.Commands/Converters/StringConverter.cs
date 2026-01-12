using CommunityToolkit.HighPerformance;
using DSharpPlus.Entities;
using Instellate.Commands.Commands;

namespace Instellate.Commands.Converters;

public class StringConverter : IConverter<string>
{
    public DiscordApplicationCommandOptionType Type => DiscordApplicationCommandOptionType.String;

    public CommandOption ConstructOption(CommandOptionMetadata metadata)
    {
        return new CommandOption(metadata.Name,
            metadata.Description,
            metadata.Optional,
            metadata.Positional)
        {
            Type = this.Type
        };
    }

    public object ConvertFromObject(object obj)
    {
        if (obj is not string)
        {
            throw new ArgumentException("Object is not string", nameof(obj));
        }

        return obj;
    }

    public object ConvertFromString(string? input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return input;
    }
}
