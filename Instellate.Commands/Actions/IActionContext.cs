using DSharpPlus;
using DSharpPlus.Entities;

namespace Instellate.Commands.Actions;

public interface IActionContext
{
    public DiscordClient Client { get; }
    public DiscordUser Author { get; }

    Task DeferAsync();

    Task CreateResponseAsync(IDiscordMessageBuilder builder, bool ephemeral = false);

    Task CreateModalResponseAsync(DiscordModalBuilder modal);
}
