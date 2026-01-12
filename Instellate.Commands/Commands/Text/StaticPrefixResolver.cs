using DSharpPlus.Entities;

namespace Instellate.Commands.Commands.Text;

public class StaticPrefixResolver : IPrefixResolver
{
    private readonly string _prefix;

    public StaticPrefixResolver(string prefix)
    {
        this._prefix = prefix;
    }

    public Task<string?> ResolvePrefixAsync(DiscordMessage message)
    {
        return Task.FromResult<string?>(this._prefix);
    }
}
