using DSharpPlus.Entities;
using Instellate.Commands.Commands.Text;

namespace Instellate.Commands.Example;

public class PrefixResolver : IPrefixResolver
{
    public Task<string?> ResolvePrefixAsync(DiscordMessage message)
    {
        return Task.FromResult<string?>("t!");
    }
}
