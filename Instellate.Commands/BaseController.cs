using DSharpPlus;
using DSharpPlus.Entities;
using Instellate.Commands.Actions;

namespace Instellate.Commands;

public abstract class BaseController
{
    public DiscordClient Client { get; internal set; } = null!;
    public DiscordUser Author { get; internal set; } = null!;
    public DiscordChannel Channel { get; internal set; } = null!;
    public IActionContext ActionContext { get; internal set; } = null!;
    public DiscordMessage? Message { get; internal set; }

    protected MessageActionResult MessageResponse(DiscordMessageBuilder message)
    {
        return new MessageActionResult(message);
    }

    protected TextActionResult Text(string content, bool ephemeral = false)
    {
        return new TextActionResult(content, ephemeral);
    }

    protected EmbedActionResult Embed()
    {
        return new EmbedActionResult();
    }

    protected EmbedActionResult Embed(DiscordEmbedBuilder builder)
    {
        return new EmbedActionResult(builder);
    }

    protected ModalActionResult Modal(DiscordModalBuilder modalBuilder)
    {
        return new ModalActionResult(modalBuilder);
    }


    protected Task DeferAsync()
    {
        return this.ActionContext.DeferAsync();
    }
}
