using DSharpPlus;
using DSharpPlus.Entities;
using Instellate.Commands.Actions;

namespace Instellate.Commands.Controllers;

public abstract class BaseController
{
    public DiscordClient Client { get; internal set; } = null!;
    public DiscordUser Author { get; internal set; } = null!;
    public DiscordChannel Channel { get; internal set; } = null!;
    public IActionContext ActionContext { get; internal set; } = null!;
    public DiscordMessage? Message { get; internal set; }

    protected TextActionResult Text(string content)
    {
        return new TextActionResult(content);
    }

    protected EmbedActionResult Embed()
    {
        return new EmbedActionResult();
    }

    protected EmbedActionResult Embed(DiscordEmbedBuilder builder)
    {
        return new EmbedActionResult(builder);
    }

    protected Task DeferAsync()
    {
        return this.ActionContext.DeferAsync();
    }
}
