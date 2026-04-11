using DSharpPlus.Entities;

namespace Instellate.Commands.Actions;

/// <summary>
/// Returns a pure text response to the user 
/// </summary>
public sealed class TextActionResult : IActionResult
{
    private readonly string _content;
    private readonly bool _ephemeral;

    public TextActionResult(string content, bool ephemeral = false)
    {
        this._content = content;
        this._ephemeral = ephemeral;
    }

    /// <inheritdoc/>
    public Task ExecuteResultAsync(IActionContext context)
    {
        DiscordMessageBuilder builder = new();
        builder.WithContent(this._content);

        return context.CreateResponseAsync(builder, this._ephemeral);
    }
}
