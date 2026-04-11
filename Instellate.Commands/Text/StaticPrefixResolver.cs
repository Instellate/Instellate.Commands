using Instellate.Commands.Actions;

namespace Instellate.Commands.Text;

/// <summary>
/// A implementation for static prefix resolver
/// </summary>
public class StaticPrefixResolver : IPrefixResolver
{
    private readonly string _prefix;

    /// <summary>
    /// Constructs a static prefix resolver
    /// </summary>
    /// <param name="prefix">The static prefix to use</param>
    public StaticPrefixResolver(string prefix)
    {
        this._prefix = prefix;
    }

    public Task<string?> ResolvePrefixAsync(IActionContext context)
    {
        return Task.FromResult<string?>(this._prefix);
    }
}
