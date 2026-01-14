using Instellate.Commands.Actions;

namespace Instellate.Commands.Text;

public interface IPrefixResolver
{
    Task<string?> ResolvePrefixAsync(IActionContext context);
}
