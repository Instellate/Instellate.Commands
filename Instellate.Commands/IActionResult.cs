using Instellate.Commands.Actions;

namespace Instellate.Commands;

/// <summary>
/// Class to handle creating easier responses to users
/// </summary>
public interface IActionResult
{
    public Task ExecuteResultAsync(IActionContext context);
}
