using DSharpPlus.Entities;

namespace Instellate.Commands.Actions;

public sealed class TextActionResult : IActionResult
{
    private readonly string _content;

    public TextActionResult(string content)
    {
        this._content = content;
    }

    public Task ExecuteResultAsync(IActionContext context)
    {
        DiscordMessageBuilder builder = new();
        builder.WithContent(this._content);

        return context.CreateResponseAsync(builder);
    }
}
