using DSharpPlus.Entities;

namespace Instellate.Commands.Actions;

/// <summary>
/// Returns a modal response to the user 
/// </summary>
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
