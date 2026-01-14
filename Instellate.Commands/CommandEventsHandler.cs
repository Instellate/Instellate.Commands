using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;

namespace Instellate.Commands;

internal static class CommandEventsHandler
{
    internal static Task HandleMessageCreatedAsync(DiscordClient client, MessageCreatedEventArgs e)
    {
        ControllerFactory factory = client.ServiceProvider.GetRequiredService<ControllerFactory>();
        return factory.HandleMessageCreatedAsync(client, e);
    }

    internal static Task HandleInteractionCreatedAsync(
        DiscordClient client,
        InteractionCreatedEventArgs e
    )
    {
        ControllerFactory factory = client.ServiceProvider.GetRequiredService<ControllerFactory>();
        return factory.HandleInteractionCreatedAsync(client, e);
    }
}
