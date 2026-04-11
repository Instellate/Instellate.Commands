using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace Instellate.Commands.Actions;

public sealed class MessageActionContext : IActionContext
{
    private int _deferred;

    /// <summary>
    ///  The message creation event arguments
    /// </summary>
    public MessageCreatedEventArgs MessageEvent { get; }

    /// <inheritdoc/>
    public DiscordClient Client { get; }

    /// <inheritdoc/>
    public DiscordUser Author => this.MessageEvent.Author;

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
    public Task CreateResponseAsync(IDiscordMessageBuilder builder, bool ephemeral)
    {
        DiscordMessageBuilder messageBuilder = new(builder);
        return this.MessageEvent.Message.RespondAsync(messageBuilder);
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
}
