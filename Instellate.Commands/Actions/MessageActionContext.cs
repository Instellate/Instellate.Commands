using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace Instellate.Commands.Actions;

public sealed class MessageActionContext : IActionContext
{
    private int _deferred;

    public MessageCreatedEventArgs MessageEvent { get; }
    public DiscordClient Client { get; }
    public DiscordUser Author => this.MessageEvent.Author;

    public MessageActionContext(MessageCreatedEventArgs messageEvent, DiscordClient client)
    {
        this.MessageEvent = messageEvent;
        this.Client = client;
    }

    public Task DeferAsync()
    {
        if (Interlocked.CompareExchange(ref this._deferred, 1, 0) == 0)
        {
            return this.MessageEvent.Channel.TriggerTypingAsync();
        }

        return Task.CompletedTask;
    }

    public Task CreateResponseAsync(IDiscordMessageBuilder builder, bool ephemeral)
    {
        DiscordMessageBuilder messageBuilder = new(builder);
        return this.MessageEvent.Message.RespondAsync(messageBuilder);
    }

    public Task CreateModalResponseAsync(DiscordModalBuilder modal)
    {
        throw new NotSupportedException();
    }
}
