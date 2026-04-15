using DSharpPlus;
using DSharpPlus.Entities;

namespace Instellate.Commands.Actions;

/// <summary>
/// The context that actions and converters can use
/// </summary>
public interface IActionContext
{
    /// <summary>
    /// The discord client used in the context
    /// </summary>
    DiscordClient Client { get; }

    /// <summary>
    /// The author of the action
    /// </summary>
    DiscordUser Author { get; }

    /// <summary>
    /// The channel tied to the action
    /// </summary>
    DiscordChannel Channel { get; }

    /// <summary>
    /// The guild tied to the action
    /// </summary>
    DiscordGuild? Guild { get; }

    /// <summary>
    /// The message tied to the action
    /// </summary>
    DiscordMessage? Message { get; }

    /// <summary>
    /// Defers the reply for later longer wait time responses
    /// </summary>
    /// <returns></returns>
    Task DeferAsync();

    /// <summary>
    /// Creates a response
    /// </summary>
    /// <param name="builder">The message builder used in the response</param>
    /// <param name="ephemeral">If the message should be ephemeral or not</param>
    /// <returns></returns>
    Task CreateResponseAsync(IDiscordMessageBuilder builder, bool ephemeral = false);

    /// <summary>
    /// Creates a modal response
    /// </summary>
    /// <param name="modal">The modal builder</param>
    /// <returns></returns>
    Task CreateModalResponseAsync(DiscordModalBuilder modal);

    /// <summary>
    /// Creates a follow up response
    /// </summary>
    /// <param name="builder">The message builder used in the response</param>
    /// <param name="ephemeral">If the message should be ephemeral or not</param>
    /// <returns></returns>
    Task CreateFollowUpResponseAsync(IDiscordMessageBuilder builder, bool ephemeral = false);

    /// <summary>
    /// Gets the original response message
    /// </summary>
    /// <returns></returns>
    Task<DiscordMessage?> GetResponseMessageAsync();
}
