using Instellate.Commands.Attributes;
using Instellate.Commands.Controllers;

namespace Instellate.Commands.Example.Commands;

[BaseController]
[Command("utility", "Commands related to utility")]
public class UtilityController : BaseController
{
    [Command("ping", "Get ping related metadata")]
    public void Execute([Option("echo", "Echoes the command")] string? echo)
    {
    }
}
