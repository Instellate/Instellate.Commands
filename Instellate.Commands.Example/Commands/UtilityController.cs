using DSharpPlus.Entities;
using Instellate.Commands.Attributes;

namespace Instellate.Commands.Example.Commands;

[BaseController]
[Command("utility", "Commands related to utility")]
public class UtilityController : BaseController
{
    public enum Test
    {
        [ChoiceName("hello")] Hello,
        There
    }

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

    [Command("modal", "Modal test")]
    public IActionResult ModalTest()
    {
        DiscordModalBuilder modal = new();
        modal
            .WithTitle("Test")
            .WithCustomId("test")
            .AddTextInput(new DiscordTextInputComponent("cool"), "Wow so cool");

        return Modal(modal);
    }

    [Command("components", "Component test")]
    public async Task<IActionResult> ComponentTestAsync()
    {
        DiscordActionRowComponent components = new(
            [
                new DiscordButtonComponent(DiscordButtonStyle.Primary, "hello", "Hello"),
                new DiscordButtonComponent(DiscordButtonStyle.Primary, "howdy", "Howdy")
            ]
        );

        DiscordMessageBuilder message = new();
        message.WithContent("Press the button for a response!")
            .AddActionRowComponent(components);

        ComponentInteractionResult? result
            = await ComponentInteractionAsync(MessageResponse(message));
        if (result is null)
        {
            return Empty();
        }

        switch (result.CustomId)
        {
            case "hello":
                return Text("Hello!");
            case "howdy":
                return Text("Howdy!");
            default:
                return Empty();
        }
    }

    [Command("enum", "Enum test")]
    public IActionResult EnumTest([Option("test", "The test enum")] Test value)
    {
        return Text($"Value: {value}");
    }

    [ContextMenu("Get User Info")]
    public async Task<string> UserInfoAsync(DiscordUser user)
    {
        await DeferAsync();
        await Task.Delay(TimeSpan.FromSeconds(1));

        return $"User: {user.Username}";
    }
}
