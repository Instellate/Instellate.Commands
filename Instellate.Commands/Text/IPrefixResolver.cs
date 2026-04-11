using Instellate.Commands.Actions;

namespace Instellate.Commands.Text;

/// <summary>
/// A interface used to implement your own prefix resolver
/// </summary>
public interface IPrefixResolver
{
    /// <summary>
    /// Called when a prefix needs to be resolved
    /// </summary>
    /// <param name="context">The current action context</param>
    /// <returns></returns>
    Task<string?> ResolvePrefixAsync(IActionContext context);
}
