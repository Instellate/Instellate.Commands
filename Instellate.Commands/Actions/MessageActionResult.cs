using DSharpPlus.Entities;

namespace Instellate.Commands.Actions;

/// <summary>
/// Returns a message response to the user 
/// </summary>
public class MessageActionResult : IActionResult
{
    private readonly DiscordMessageBuilder _message;

    public MessageActionResult(DiscordMessageBuilder message)
    {
        this._message = message;
    }

    /// <inheritdoc/>
    public Task ExecuteResultAsync(IActionContext context)
    {
        return context.CreateResponseAsync(this._message);
    }
}
