namespace Instellate.Commands.Actions;

public class EmptyActionResult : IActionResult
{
    public Task ExecuteResultAsync(IActionContext context)
    {
        return Task.CompletedTask;
    }
}
