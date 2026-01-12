using DSharpPlus.Entities;

namespace Instellate.Commands.Actions;

public class ModalActionResult : DiscordModalBuilder, IActionResult
{
    public Task ExecuteResultAsync(IActionContext context)
    {
        return context.CreateModalResponseAsync(this);
    }
}
