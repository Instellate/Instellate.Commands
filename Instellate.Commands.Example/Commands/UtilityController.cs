using DSharpPlus.Entities;
using Instellate.Commands.Attributes;
using Instellate.Commands.Controllers;

namespace Instellate.Commands.Example.Commands;

[BaseController]
[Command("utility", "Commands related to utility")]
public class UtilityController : BaseController
{
    [Command("echo", "Echo a message")]
    public IActionResult Echo([Option("content", "The content to echo")] string content)
    {
        return Text(content);
    }

    [Command("ping", "Get various ping information about the bot")]
    public IActionResult Ping()
    {
        TimeSpan latency = this.Client.GetConnectionLatency(this.Channel.GuildId ?? 0);
        double milliLatency = Math.Round(latency.TotalMilliseconds);

        return Embed()
            .WithTitle("Pong!")
            .AddField("Gateway latency", $"{milliLatency}ms", false);
    }

    [Command("wait", "Deferring test")]
    public async Task<IActionResult> WaitAsync()
    {
        await DeferAsync();
        await Task.Delay(TimeSpan.FromSeconds(3));
        return Text("Waited!");
    }

    [Command("number", "Number test")]
    public string NumberTest([Option("num", "A integer")] int num)
    {
        return $"Integer provided: {num}";
    }

    [ContextMenu("Get User Info")]
    public async Task<string> UserInfoAsync(DiscordUser user)
    {
        await DeferAsync();
        await Task.Delay(TimeSpan.FromSeconds(1));

        return $"User: {user.Username}";
    }
}
