using Instellate.Commands.Actions;

namespace Instellate.Commands;

public interface IActionResult
{
    public Task ExecuteResultAsync(IActionContext context);
}
