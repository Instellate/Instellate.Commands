using DSharpPlus.Entities;
using Instellate.Commands.Commands;

namespace Instellate.Commands.Converters;

public interface IConverter
{
    DiscordApplicationCommandOptionType Type { get; }

    CommandOption ConstructOption(CommandOptionMetadata metadata);

    object? ConvertFromObject(object? obj);

    /// <summary>
    /// Converts a string input.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    object? ConvertFromString(Optional<string> input);
}
