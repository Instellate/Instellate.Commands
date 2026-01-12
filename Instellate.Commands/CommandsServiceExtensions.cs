using System.Reflection;
using DSharpPlus;
using Instellate.Commands.Attributes;
using Instellate.Commands.Converters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Instellate.Commands;

public static class CommandsServiceExtensions
{
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

        collection.AddSingleton<ControllerFactory>()
            .AddSingleton<IConverter<string>, StringConverter>()
            .AddSingleton<IConverter<int>, Int32Converter>()
            .AddSingleton<IConverter<bool>, BoolConverter>();
        return collection;
    }

    public static IServiceProvider MapCommandControllers(this IServiceProvider provider)
    {
        using IServiceScope scope = provider.CreateScope();

        ControllerFactory factory = scope.ServiceProvider.GetRequiredService<ControllerFactory>();
        factory.MapControllers(Assembly.GetCallingAssembly());

        return provider;
    }

    public static Task RegisterApplicationCommands(this DiscordClient client,
        ulong? debugGuildId = null)
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
}
