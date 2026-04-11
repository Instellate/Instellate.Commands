namespace Instellate.Commands.Actions;

/// <summary>
/// Returns nothing
/// </summary>
public class EmptyActionResult : IActionResult
{
    public Task ExecuteResultAsync(IActionContext context)
    {
        return Task.CompletedTask;
    }
}
