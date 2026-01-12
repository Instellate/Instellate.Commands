using DSharpPlus.Entities;
using Instellate.Commands.Attributes;
using Instellate.Commands.Attributes.Application;
using Instellate.Commands.Controllers;

namespace Instellate.Commands.Example.Commands;

[BaseController]
public class ContextMenuController : BaseController
{
    [Command("Get User info")]
    [ApplicationType(DiscordApplicationCommandType.UserContextMenu)]
    public async Task<IActionResult> UserInfoAsync()
    {
        await DeferAsync();
        return Text("Test", true);
    }
}
