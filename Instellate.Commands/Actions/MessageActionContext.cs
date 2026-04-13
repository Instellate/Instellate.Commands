using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace Instellate.Commands.Actions;

public sealed class MessageActionContext : IActionContext
{
    private int _deferred;
    private DiscordMessage? _message = null;

    /// <summary>
    ///  The message creation event arguments
    /// </summary>
    public MessageCreatedEventArgs MessageEvent { get; }

    /// <inheritdoc/>
    public DiscordClient Client { get; }

    /// <inheritdoc/>
    public DiscordUser Author => this.MessageEvent.Author;

    // ReSharper disable once ReturnTypeCanBeNotNullable Can be null
    /// <inheritdoc/>
    public DiscordGuild? Guild => this.MessageEvent.Guild;

    public MessageActionContext(MessageCreatedEventArgs messageEvent, DiscordClient client)
    {
        this.MessageEvent = messageEvent;
        this.Client = client;
    }

    /// <inheritdoc/>
    public Task DeferAsync()
    {
        if (Interlocked.CompareExchange(ref this._deferred, 1, 0) == 0)
        {
            return this.MessageEvent.Channel.TriggerTypingAsync();
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task CreateResponseAsync(IDiscordMessageBuilder builder, bool ephemeral)
    {
        DiscordMessageBuilder messageBuilder = new(builder);
        this._message = await this.MessageEvent.Message.RespondAsync(messageBuilder);
    }

    /// <summary>
    /// Not supported for message actions
    /// </summary>
    /// <param name="modal"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    public Task CreateModalResponseAsync(DiscordModalBuilder modal)
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc/>
    public Task CreateFollowUpResponseAsync(IDiscordMessageBuilder builder, bool ephemeral = false)
    {
        DiscordMessageBuilder messageBuilder = new(builder);
        if (this._message is null)
        {
            throw new CommandsException(
                "Cannot create a follow up when the initial response has not been created"
            );
        }

        return this._message.RespondAsync(messageBuilder);
    }

    /// <inheritdoc/>
    public Task<DiscordMessage?> GetResponseMessageAsync()
    {
        return Task.FromResult(this._message);
    }
}
