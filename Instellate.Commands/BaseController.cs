using System.Threading.Tasks.Dataflow;
using DSharpPlus;
using DSharpPlus.Entities;
using Instellate.Commands.Actions;
using Microsoft.Extensions.DependencyInjection;

namespace Instellate.Commands;

/// <summary>
/// Base controller for commands to derive
/// </summary>
public abstract class BaseController
{
    public DiscordClient Client { get; internal set; } = null!;
    public DiscordUser Author { get; internal set; } = null!;
    public DiscordChannel Channel { get; internal set; } = null!;
    public IActionContext ActionContext { get; internal set; } = null!;
    public DiscordMessage? Message { get; internal set; }

    /// <summary>
    /// Create an action result for dicord message that derives content from a message builder
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    protected MessageActionResult MessageResponse(DiscordMessageBuilder message)
    {
        return new MessageActionResult(message);
    }

    /// <summary>
    /// Create an action result for pure text content
    /// </summary>
    /// <param name="content">The text content to return to the user</param>
    /// <param name="ephemeral">If the action should be ephemeral or not</param>
    /// <returns></returns>
    protected TextActionResult Text(string content, bool ephemeral = false)
    {
        return new TextActionResult(content, ephemeral);
    }

    /// <summary>
    /// Create an action result for a discord embed content
    /// </summary>
    /// <returns></returns>
    protected EmbedActionResult Embed()
    {
        return new EmbedActionResult();
    }

    /// <summary>
    /// Create a action result for a discord embed that derives content from an embed builder
    /// </summary>
    /// <param name="builder">The builder to derive content from</param>
    /// <returns></returns>
    protected EmbedActionResult Embed(DiscordEmbedBuilder builder)
    {
        return new EmbedActionResult(builder);
    }

    /// <summary>
    /// Create an action result for dicord modal that derives content from a modal builder
    /// </summary>
    /// <param name="modalBuilder"></param>
    /// <returns></returns>
    protected ModalActionResult Modal(DiscordModalBuilder modalBuilder)
    {
        return new ModalActionResult(modalBuilder);
    }

    /// <summary>
    /// Creates an action result that causes nothing
    /// </summary>
    /// <returns></returns>
    protected EmptyActionResult Empty()
    {
        return new EmptyActionResult();
    }

    protected async Task<ComponentInteractionResult?> ComponentInteractionAsync(
        IActionResult result,
        TimeSpan? expiry = null
    )
    {
        expiry ??= TimeSpan.FromSeconds(5);

        await result.ExecuteResultAsync(this.ActionContext);

        CancellationTokenSource ctSource = new();
        ctSource.CancelAfter(expiry.Value);
        BufferBlock<InteractionActionContext> bufferBlock = new();

        ControllerFactory factory = Client.ServiceProvider.GetRequiredService<ControllerFactory>();
        DiscordMessage? message = await this.ActionContext.GetResponseMessageAsync();
        if (message is null)
        {
            throw new CommandsException(
                "Cannot wait for component interaction without a response message"
            );
        }

        try
        {
            factory._componentBlocks.TryAdd(message.Id, bufferBlock);
            InteractionActionContext newContext
                = await bufferBlock.ReceiveAsync(cancellationToken: ctSource.Token);

            this.ActionContext = newContext;

            return new ComponentInteractionResult(
                newContext.Interaction.Data.CustomId,
                newContext.Interaction.Data.Values
            );
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            factory._componentBlocks.TryRemove(message.Id, out _);
        }

        return null;
    }

    /// <summary>
    /// Defers the message for a later reply
    /// </summary>
    /// <returns></returns>
    protected Task DeferAsync()
    {
        return this.ActionContext.DeferAsync();
    }
}
