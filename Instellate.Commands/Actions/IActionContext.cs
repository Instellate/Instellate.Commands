using DSharpPlus.Entities;

namespace Instellate.Commands.Actions;

public interface IActionContext
{
    Task DeferAsync();

    Task CreateResponseAsync(IDiscordMessageBuilder builder, bool ephemeral = false);

    Task CreateModalResponseAsync(DiscordModalBuilder modal);
}
