using DSharpPlus;
using DSharpPlus.Entities;

namespace Instellate.Commands.Actions;

public sealed class InteractionActionContext : IActionContext
{
    private int _deferred = 0;

    public DiscordInteraction Interaction { get; }
    public DiscordClient Client { get; }
    public DiscordUser Author => Interaction.User;

    public InteractionActionContext(DiscordInteraction interaction, DiscordClient client)
    {
        this.Interaction = interaction;
        this.Client = client;
    }

    public Task DeferAsync()
    {
        if (Interlocked.CompareExchange(ref this._deferred, 1, 0) == 0)
        {
            return this.Interaction.DeferAsync();
        }
        else
        {
            return Task.CompletedTask;
        }
    }

    public Task CreateResponseAsync(IDiscordMessageBuilder builder, bool ephemeral)
    {
        if (this._deferred == 1)
        {
            DiscordFollowupMessageBuilder followupBuilder = new(builder);
            followupBuilder.AsEphemeral(ephemeral);
            return this.Interaction.CreateFollowupMessageAsync(followupBuilder);
        }
        else
        {
            DiscordInteractionResponseBuilder responseBuilder = new(builder);
            responseBuilder.AsEphemeral(ephemeral);
            return this.Interaction.CreateResponseAsync(
                DiscordInteractionResponseType.ChannelMessageWithSource,
                responseBuilder);
        }
    }

    public Task CreateModalResponseAsync(DiscordModalBuilder modal)
    {
        return this.Interaction.CreateResponseAsync(
            DiscordInteractionResponseType.DeferredChannelMessageWithSource,
            modal);
    }
}
