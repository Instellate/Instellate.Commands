using DSharpPlus;
using DSharpPlus.EventArgs;
using Instellate.Commands.Commands.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Instellate.Commands.Example;

public static class Program
{
    public static async Task Main()
    {
        IConfiguration config = new ConfigurationBuilder()
            .AddUserSecrets(typeof(Program).Assembly)
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        DiscordClientBuilder builder
            = DiscordClientBuilder.CreateSharded(config["BOT_TOKEN"]!,
                DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents);

        builder
            .ConfigureServices(ConfigureServices)
            .ConfigureEventHandlers(e =>
                e.HandleCommandEvents().HandleSessionCreated(HandleSessionCreatedAsync))
            .ConfigureLogging(l =>
                l.AddConsole().AddConfiguration(config.GetSection("Logging")));

        DiscordClient client = builder.Build();
        client.ServiceProvider.MapCommandControllers();
        await client.ConnectAsync();

        client.Logger.LogInformation("Registering application commands");
        await client.RegisterApplicationCommands(config.GetValue<ulong>("GUILD_ID"));

        await Task.Delay(-1);
    }

    private static void ConfigureServices(IServiceCollection collection)
    {
        collection
            .AddCommands()
            .AddSingleton<IPrefixResolver, PrefixResolver>();
    }

    private static Task HandleSessionCreatedAsync(DiscordClient client, SessionCreatedEventArgs e)
    {
        client.Logger.LogInformation("Shard {ShardId} created, taking care of {GuildCount} guilds",
            e.ShardId,
            e.GuildIds.Count);
        return Task.CompletedTask;
    }
}
