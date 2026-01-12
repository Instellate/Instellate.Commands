using DSharpPlus.Entities;

namespace Instellate.Commands.Commands.Text;

public interface IPrefixResolver
{
    Task<string?> ResolvePrefixAsync(DiscordMessage message);
}
