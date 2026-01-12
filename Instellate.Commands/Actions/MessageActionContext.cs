using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace Instellate.Commands.Actions;

internal sealed class MessageActionContext : IActionContext
{
    private readonly MessageCreatedEventArgs _messageEvent;
    private int _deferred = 0;

    public MessageActionContext(MessageCreatedEventArgs messageEvent)
    {
        this._messageEvent = messageEvent;
    }

    public Task DeferAsync()
    {
        if (Interlocked.CompareExchange(ref this._deferred, 1, 0) == 0)
        {
            return this._messageEvent.Channel.TriggerTypingAsync();
        }

        return Task.CompletedTask;
    }

    public Task CreateResponseAsync(IDiscordMessageBuilder builder, bool ephemeral)
    {
        DiscordMessageBuilder messageBuilder = new(builder);
        return this._messageEvent.Message.RespondAsync(messageBuilder);
    }

    public Task CreateModalResponseAsync(DiscordModalBuilder modal)
    {
        throw new NotSupportedException();
    }
}
