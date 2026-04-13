using System.Text.RegularExpressions;
using DSharpPlus.Entities;
using Instellate.Commands.Actions;
using Instellate.Commands.Commands;

namespace Instellate.Commands.Converters;

public partial class DiscordRoleConverter : IConverter<DiscordRole>
{
    public DiscordApplicationCommandOptionType Type => DiscordApplicationCommandOptionType.Role;

    public CommandOption ConstructOption(CommandOptionMetadata metadata)
    {
        return new CommandOption(
            metadata.Name,
            metadata.Description,
            metadata.Optional,
            metadata.Positional
        )
        {
            Type = this.Type
        };
    }

    public ValueTask<object?> ConvertFromObject(object? obj, IActionContext context)
    {
        if (obj is not ulong roleId)
        {
            throw new ArgumentException("Object is not ulong", nameof(obj));
        }

        if (context is InteractionActionContext interaction)
        {
            DiscordRole roles
                = interaction.Interaction.Data.Resolved.Roles[roleId];
            return ValueTask.FromResult<object?>(roles);
        }

        if (context.Guild is null)
        {
            throw new ArgumentNullException(nameof(context), "Property guild is null");
        }

        return ValueTask.FromResult<object?>(context.Guild.Roles.GetValueOrDefault(roleId)!);
    }

    public ValueTask<object?> ConvertFromString(
        Optional<string> input,
        IActionContext context
    )
    {
        if (!input.TryGetValue(out string? value))
        {
            throw new ArgumentNullException(nameof(input));
        }

        if (context.Guild is null)
        {
            throw new ArgumentNullException(nameof(context), "Property `Guild` is null");
        }

        if (ulong.TryParse(value, out ulong roleId))
        {
            return ValueTask.FromResult<object?>(
                context.Guild.Roles.GetValueOrDefault(roleId)!
            );
        }

        Regex roleMentionRegex = RoleMentionRegex();
        Match match = roleMentionRegex.Match(value);
        if (match.Success)
        {
            roleId = ulong.Parse(match.Groups[1].Value);
            return ValueTask.FromResult<object?>(
                context.Guild.Roles.GetValueOrDefault(roleId)!
            );
        }

        throw new ArgumentException("Input does not follow role format", nameof(input));
    }

    [GeneratedRegex(@"^<@&(\d+)>$")]
    private static partial Regex RoleMentionRegex();
}
