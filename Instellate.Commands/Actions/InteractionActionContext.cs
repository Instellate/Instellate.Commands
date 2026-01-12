using System.Reflection;
using DSharpPlus.Entities;

namespace Instellate.Commands.Actions;

internal class InteractionActionContext : IActionContext
{
    private readonly DiscordInteraction _interaction;
    private int _deferred = 0;

    public InteractionActionContext(DiscordInteraction interaction)
    {
        this._interaction = interaction;
    }

    public Task DeferAsync()
    {
        if (Interlocked.CompareExchange(ref this._deferred, 1, 0) == 0)
        {
            return this._interaction.DeferAsync();
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
            return this._interaction.CreateFollowupMessageAsync(followupBuilder);
        }
        else
        {
            DiscordInteractionResponseBuilder responseBuilder = new(builder);
            responseBuilder.AsEphemeral(ephemeral);
            return this._interaction.CreateResponseAsync(
                DiscordInteractionResponseType.ChannelMessageWithSource,
                responseBuilder);
        }
    }

    public Task CreateModalResponseAsync(DiscordModalBuilder modal)
    {
        return this._interaction.CreateResponseAsync(
            DiscordInteractionResponseType.DeferredChannelMessageWithSource,
            modal);
    }
}
