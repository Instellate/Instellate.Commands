using DSharpPlus.Entities;
using Instellate.Commands.Actions;
using Instellate.Commands.Commands;

namespace Instellate.Commands.Converters;

/// <summary>
/// Interface for implementing a converter
/// </summary>
public interface IConverter
{
    /// <summary>
    /// The application command option type that this converter resolves to 
    /// </summary>
    DiscordApplicationCommandOptionType Type { get; }

    /// <summary>
    /// Called to construct a command application option
    /// </summary>
    /// <param name="metadata">The metadata related to the option being constructed</param>
    /// <returns></returns>
    CommandOption ConstructOption(CommandOptionMetadata metadata);

    /// <summary>
    /// Handle convertion from a application command
    /// </summary>
    /// <param name="obj">The value represented as object, given as null if non is given</param>
    /// <param name="context">The current context for the command being executed</param>
    /// <returns>The object being returned, null means no value should be given</returns>
    ValueTask<object?> ConvertFromObject(object? obj, IActionContext context);

    /// <summary>
    /// Handle convertion fom a string command
    /// </summary>
    /// <param name="input">The value represented as a string, given as null if non is given</param>
    /// <param name="context">The current context for the command being executed</param>
    /// <returns>The object being returned, null means no value should be given</returns>
    ValueTask<object?> ConvertFromString(Optional<string> input, IActionContext context);
}
