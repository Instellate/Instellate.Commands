using DSharpPlus.Clients;
using Instellate.Commands.Attributes;
using Instellate.Commands.Attributes.Text;
using Instellate.Commands.Controllers;

namespace Instellate.Commands.Example.Commands;

[BaseController]
[Command("utility", "Commands related to utility")]
public class UtilityController : BaseController
{
    private readonly IShardOrchestrator _orchestrator;

    public UtilityController(IShardOrchestrator orchestrator)
    {
        this._orchestrator = orchestrator;
    }

    [Command("echo", "Echo a message")]
    public IActionResult Echo([Option("content", "The content to echo")] string content)
    {
        return Text(content);
    }

    [Command("ping", "Get various ping information about the bot")]
    public IActionResult Ping()
    {
        TimeSpan latency;
        if (this.Channel.GuildId is { } guildId)
        {
            latency = this._orchestrator.GetConnectionLatency(guildId);
        }
        else
        {
            latency = this._orchestrator.GetConnectionLatency(0);
        }

        return Embed()
            .WithTitle("Pong!")
            .AddField("Gateway latency", $"{Math.Round(latency.TotalMilliseconds)}ms", false);
    }

    [Command("test", "Test command")]
    public IActionResult Test([Option("test", "Test option")] bool test)
    {
        return Text(test ? "Yes" : "No");
    }
}
