using System.Reflection;
using DSharpPlus;
using DSharpPlus.Entities;
using Instellate.Commands.Attributes;
using Instellate.Commands.Converters;
using Instellate.Commands.Text;
using Microsoft.Extensions.DependencyInjection;

namespace Instellate.Commands;

public static class CommandsServiceExtensions
{
    /// <summary>
    /// Find all command controllers and map them to commands
    /// </summary>
    /// <param name="client"></param>
    /// <returns></returns>
    public static DiscordClient MapCommandControllers(this DiscordClient client)
    {
        ControllerFactory factory = client.ServiceProvider.GetRequiredService<ControllerFactory>();
        factory.MapControllers(Assembly.GetCallingAssembly());
        return client;
    }

    /// <summary>
    /// Register all application commands to discord
    /// </summary>
    /// <param name="client"></param>
    /// <param name="debugGuildId">Guild id to register it to, all commands will be registered public if null is specified</param>
    /// <returns></returns>
    public static Task RegisterApplicationCommands(
        this DiscordClient client,
        ulong? debugGuildId = null
    )
    {
        ControllerFactory factory = client.ServiceProvider.GetRequiredService<ControllerFactory>();
        return factory.RegisterCommandsAsync(client, debugGuildId);
    }

    /// <summary>
    /// Register handling of command events
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static EventHandlingBuilder HandleCommandEvents(this EventHandlingBuilder builder)
    {
        return builder
            .HandleMessageCreated(CommandEventsHandler.HandleMessageCreatedAsync)
            .HandleInteractionCreated(CommandEventsHandler.HandleInteractionCreatedAsync);
    }

    /// <summary>
    /// Finds and registers all command controllers into the discord client service collection
    /// Also registers all basic converters 
    /// </summary>
    /// <param name="collection"></param>
    /// <returns></returns>
    public static IServiceCollection AddCommands(this IServiceCollection collection)
    {
        Assembly assembly = Assembly.GetCallingAssembly();

        foreach (Type type in assembly.GetExportedTypes())
        {
            if (type.GetCustomAttribute<BaseControllerAttribute>() is null)
            {
                continue;
            }

            collection.AddScoped(type);
        }

        collection
            .AddSingleton<ControllerFactory>()
            .AddConverter<StringConverter, string>()
            .AddConverter<IntegerConverter<short>, short>()
            .AddConverter<IntegerConverter<int>, int>()
            .AddConverter<IntegerConverter<long>, long>()
            .AddConverter<IntegerConverter<ushort>, ushort>()
            .AddConverter<IntegerConverter<uint>, uint>()
            .AddConverter<DoubleConverter, double>()
            .AddConverter<BoolConverter, bool>()
            .AddConverter<DiscordUserConverter, DiscordUser>()
            .AddConverter<DiscordChannelConverter, DiscordChannel>()
            .AddConverter<DiscordRoleConverter, DiscordRole>()
            .AddConverter<DiscordMemberConverter, DiscordMember>()
            .AddConverter<SnowflakeObjectConverter, SnowflakeObject>();

        return collection;
    }

    public static IServiceCollection AddConverter<TConverter, TValue>(
        this IServiceCollection collection
    )
        where TConverter : class, IConverter<TValue>
    {
        return collection.AddSingleton<IConverter<TValue>, TConverter>();
    }

    /// <summary>
    /// Adds a static prefix resolver
    /// </summary>
    /// <param name="collection"></param>
    /// <param name="prefix">The static prefix to use</param>
    /// <returns></returns>
    public static IServiceCollection AddStaticPrefixResolver(
        this IServiceCollection collection,
        string prefix
    )
    {
        collection.AddSingleton<IPrefixResolver, StaticPrefixResolver>(_ =>
            new StaticPrefixResolver(prefix)
        );
        return collection;
    }
}
