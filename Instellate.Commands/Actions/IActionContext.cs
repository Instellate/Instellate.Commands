using DSharpPlus.Entities;

namespace Instellate.Commands.Actions;

public interface IActionContext
{
    public Task DeferAsync();

    public Task CreateResponseAsync(IDiscordMessageBuilder builder);
}
