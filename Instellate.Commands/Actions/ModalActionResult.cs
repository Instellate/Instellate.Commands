using DSharpPlus.Entities;

namespace Instellate.Commands.Actions;

public class ModalActionResult : IActionResult
{
    private readonly DiscordModalBuilder _modalBuilder;

    public ModalActionResult(DiscordModalBuilder modalBuilder)
    {
        this._modalBuilder = modalBuilder;
    }

    public Task ExecuteResultAsync(IActionContext context)
    {
        return context.CreateModalResponseAsync(this._modalBuilder);
    }
}
