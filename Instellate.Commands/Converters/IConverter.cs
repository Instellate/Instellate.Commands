using DSharpPlus.Entities;
using Instellate.Commands.Commands;

namespace Instellate.Commands.Converters;

public interface IConverter
{
    DiscordApplicationCommandOptionType Type { get; }

    CommandOption ConstructOption(CommandOptionMetadata metadata);

    object? ConvertFromObject(object obj);

    object? ConvertFromString(string? input);
}
