using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

namespace Instellate.Commands.Actions;

/// <summary>
/// A action context for interactions
/// </summary>
public sealed class InteractionActionContext : IActionContext
{
    private int _deferred;

    /// <summary>
    /// The interaction used in the response
    /// </summary>
    public DiscordInteraction Interaction { get; }

    /// <inheritdoc/>
    public DiscordClient Client { get; }

    /// <inheritdoc/>
    public DiscordUser Author => this.Interaction.User;

    /// <inheritdoc/>
    public DiscordChannel Channel => this.Interaction.Channel;

    /// <inheritdoc/>
    public DiscordMessage? Message => this.Interaction.Message;

    /// <inheritdoc/>
    public DiscordGuild? Guild => this.Interaction.Guild;

    public InteractionActionContext(DiscordInteraction interaction, DiscordClient client)
    {
        this.Interaction = interaction;
        this.Client = client;
    }

    /// <inheritdoc/>
    public Task DeferAsync()
    {
        if (Interlocked.CompareExchange(ref this._deferred, 1, 0) == 0)
        {
            return this.Interaction.DeferAsync();
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task CreateResponseAsync(IDiscordMessageBuilder builder, bool ephemeral)
    {
        if (Interlocked.CompareExchange(ref this._deferred, 1, 1) == 1)
        {
            DiscordFollowupMessageBuilder followupBuilder = new(builder);
            followupBuilder.AsEphemeral(ephemeral);
            return this.Interaction.CreateFollowupMessageAsync(followupBuilder);
        }

        DiscordInteractionResponseBuilder responseBuilder = new(builder);
        responseBuilder.AsEphemeral(ephemeral);
        return this.Interaction.CreateResponseAsync(
            DiscordInteractionResponseType.ChannelMessageWithSource,
            responseBuilder
        );
    }

    /// <inheritdoc/>
    public Task CreateModalResponseAsync(DiscordModalBuilder modal)
    {
        return this.Interaction.CreateResponseAsync(DiscordInteractionResponseType.Modal, modal);
    }

    /// <inheritdoc/>
    public Task CreateFollowUpResponseAsync(IDiscordMessageBuilder builder, bool ephemeral = false)
    {
        DiscordFollowupMessageBuilder followupBuilder = new(builder);
        followupBuilder.AsEphemeral(ephemeral);
        return this.Interaction.CreateFollowupMessageAsync(followupBuilder);
    }

    /// <inheritdoc/>
    public async Task<DiscordMessage?> GetResponseMessageAsync()
    {
        try
        {
            return await this.Interaction.GetOriginalResponseAsync();
        }
        catch (NotFoundException)
        {
            return null;
        }
    }
}
