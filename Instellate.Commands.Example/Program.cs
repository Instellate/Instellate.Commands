using Instellate.Commands.Converters;
using Microsoft.Extensions.DependencyInjection;

namespace Instellate.Commands.Example;

public static class Program
{
    public static void Main()
    {
        ServiceCollection collection = new();
        collection
            .AddLogging()
            .AddCommandControllers()
            .AddSingleton<IConverter<string>, StringConverter>()
            .AddSingleton<IConverter<int>, Int32Converter>();

        IServiceProvider provider = collection.BuildServiceProvider();
        provider.MapCommandControllers();
    }
}
