using DSharpPlus.Entities;
using Instellate.Commands.Actions;
using Instellate.Commands.Commands;

namespace Instellate.Commands.Converters;

public interface IConverter
{
    DiscordApplicationCommandOptionType Type { get; }

    CommandOption ConstructOption(CommandOptionMetadata metadata);

    ValueTask<object?> ConvertFromObject(object? obj, IActionContext context);

    ValueTask<object?> ConvertFromString(Optional<string> input, IActionContext context);
}
