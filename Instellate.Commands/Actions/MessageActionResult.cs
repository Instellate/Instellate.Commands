using DSharpPlus.Entities;

namespace Instellate.Commands.Actions;

public class MessageActionResult : IActionResult
{
    private readonly DiscordMessageBuilder _message;

    public MessageActionResult(DiscordMessageBuilder message)
    {
        this._message = message;
    }

    public Task ExecuteResultAsync(IActionContext context)
    {
        return context.CreateResponseAsync(this._message);
    }
}
