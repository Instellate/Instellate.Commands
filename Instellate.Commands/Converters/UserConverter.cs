using System.Text.RegularExpressions;
using DSharpPlus.Entities;
using Instellate.Commands.Actions;
using Instellate.Commands.Commands;

namespace Instellate.Commands.Converters;

public partial class UserConverter : IConverter<DiscordUser>
{
    [GeneratedRegex(@"^<@!?(\d+)>$", RegexOptions.Compiled)]
    private static partial Regex UserMentionRegex();

    // ReSharper disable once UnusedMember.Local Might have use late
    [GeneratedRegex(@"^(?:[a-z]|\.[a-z_])+\.?$", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex UsernameRegex();

    public DiscordApplicationCommandOptionType Type => DiscordApplicationCommandOptionType.User;

    public CommandOption ConstructOption(CommandOptionMetadata metadata)
    {
        return new CommandOption(metadata.Name,
            metadata.Description,
            metadata.Optional,
            metadata.Positional)
        {
            Type = this.Type
        };
    }

    public async ValueTask<object?> ConvertFromObject(object? obj, IActionContext context)
    {
        if (obj is not ulong userId)
        {
            throw new ArgumentException("Object is not ulong", nameof(obj));
        }

        if (context is InteractionActionContext interaction)
        {
            DiscordUser user
                = interaction.Interaction.Data.Resolved.Users[userId];
            return user;
        }

        return await context.Client.GetUserAsync(userId);
    }

    public async ValueTask<object?> ConvertFromString(Optional<string> input,
        IActionContext context)
    {
        if (!input.TryGetValue(out string? value))
        {
            throw new ArgumentNullException(nameof(input));
        }

        if (ulong.TryParse(value, out ulong userId))
        {
            return await context.Client.GetUserAsync(userId);
        }

        Regex userMention = UserMentionRegex();
        Match match = userMention.Match(value);
        if (match.Success)
        {
            userId = ulong.Parse(match.Groups[1].Value);
            return await context.Client.GetUserAsync(userId);
        }
        else
        {
            throw new ArgumentException("Input does not follow user format", nameof(input));
        }
    }
}
