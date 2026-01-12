using System.Reflection;
using Instellate.Commands.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Instellate.Commands;

public static class CommandsServiceExtensions
{
    public static IServiceCollection AddCommandControllers(this IServiceCollection collection)
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

        collection.AddSingleton<ControllerFactory>();
        return collection;
    }

    public static IServiceProvider MapCommandControllers(this IServiceProvider provider)
    {
        using IServiceScope scope = provider.CreateScope();
        
        ControllerFactory factory = scope.ServiceProvider.GetRequiredService<ControllerFactory>();
        factory.MapControllers(Assembly.GetCallingAssembly());
        
        return provider;
    }
}
