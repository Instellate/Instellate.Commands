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
    public static DiscordClient MapCommandControllers(this DiscordClient client)
    {
        ControllerFactory factory = client.ServiceProvider.GetRequiredService<ControllerFactory>();
        factory.MapControllers(Assembly.GetCallingAssembly());
        return client;
    }

    public static Task RegisterApplicationCommands(
        this DiscordClient client,
        ulong? debugGuildId = null
    )
    {
        ControllerFactory factory = client.ServiceProvider.GetRequiredService<ControllerFactory>();
        return factory.RegisterCommandsAsync(client, debugGuildId);
    }

    public static EventHandlingBuilder HandleCommandEvents(this EventHandlingBuilder builder)
    {
        return builder
            .HandleMessageCreated(CommandEventsHandler.HandleMessageCreatedAsync)
            .HandleInteractionCreated(CommandEventsHandler.HandleInteractionCreatedAsync);
    }

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
            .AddConverter<Int32Converter, int>()
            .AddConverter<Int64Converter, long>()
            .AddConverter<DoubleConverter, double>()
            .AddConverter<BoolConverter, bool>()
            .AddConverter<DiscordUserConverter, DiscordUser>()
            .AddConverter<DiscordChannelConverter, DiscordChannel>();

        return collection;
    }

    internal static IServiceCollection AddConverter<TConverter, TValue>(
        this IServiceCollection collection
    )
        where TConverter : class, IConverter<TValue>
    {
        return collection.AddSingleton<IConverter<TValue>, TConverter>();
    }

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
