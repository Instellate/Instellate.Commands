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
    public DiscordClient Client { get; }

    /// <summary>
    /// The author of the message
    /// </summary>
    public DiscordUser Author { get; }

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
}
