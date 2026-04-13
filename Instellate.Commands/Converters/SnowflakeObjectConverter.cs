using System.Text.RegularExpressions;
using DSharpPlus.Entities;
using Instellate.Commands.Actions;
using Instellate.Commands.Commands;

namespace Instellate.Commands.Converters;

public partial class SnowflakeObjectConverter : IConverter<SnowflakeObject>
{
    private static readonly DiscordRoleConverter _roleConverter = new();
    private static readonly DiscordMemberConverter _memberConverter = new();
    private static readonly DiscordUserConverter _userConverter = new();

    public DiscordApplicationCommandOptionType Type =>
        DiscordApplicationCommandOptionType.Mentionable;

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
        try
        {
            return _roleConverter.ConvertFromObject(obj, context);
        }
        catch (Exception)
        {
            // ignored
        }

        try
        {
            return _memberConverter.ConvertFromObject(obj, context);
        }
        catch (Exception)
        {
            // ignored
        }

        try
        {
            return _userConverter.ConvertFromObject(obj, context);
        }
        catch (Exception)
        {
            // ignored
        }

        throw new ArgumentException("Couldn't find a mentionable", nameof(obj));
    }

    public async ValueTask<object?> ConvertFromString(
        Optional<string> input,
        IActionContext context
    )
    {
        if (!input.TryGetValue(out string? value))
        {
            throw new ArgumentNullException(nameof(input));
        }

        if (ulong.TryParse(value, out ulong mentionableId))
        {
            if (context.Guild is not null)
            {
                if (context.Guild.Roles.TryGetValue(mentionableId, out DiscordRole? role))
                {
                    return role;
                }

                if (context.Guild.Members.TryGetValue(mentionableId, out DiscordMember? member))
                {
                    return member;
                }
            }

            return await context.Client.GetUserAsync(mentionableId);
        }

        Regex mentionableRegex = MentionableRegex();
        Match match = mentionableRegex.Match(value);
        if (match.Success)
        {
            ReadOnlySpan<char> mentionableType = match.Groups[1].ValueSpan;
            mentionableId = ulong.Parse(match.Groups[2].ValueSpan);

            if (mentionableType is "@&")
            {
                if (context.Guild is null)
                {
                    throw new ArgumentNullException(nameof(context), "Property `Guild` is null");
                }

                return context.Guild.Roles[mentionableId];
            }
            else
            {
                if (context.Guild is not null)
                {
                    if (context.Guild.Members.TryGetValue(mentionableId, out DiscordMember? member))
                    {
                        return member;
                    }

                    return await context.Client.GetUserAsync(mentionableId);
                }
            }
        }

        throw new ArgumentException("Input does not follow mentionable format", nameof(input));
    }

    [GeneratedRegex(@"^<(@!?|@&)(\d+)>$")]
    private static partial Regex MentionableRegex();
}
