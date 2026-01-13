using System.Text.RegularExpressions;
using DSharpPlus.Entities;
using Instellate.Commands.Actions;
using Instellate.Commands.Commands;

namespace Instellate.Commands.Converters;

public partial class ChannelConverter : IConverter<DiscordChannel>
{
    [GeneratedRegex(@"^<#!?(\d+)>$", RegexOptions.Compiled)]
    private static partial Regex ChannelMentionRegex();

    public DiscordApplicationCommandOptionType Type => DiscordApplicationCommandOptionType.Channel;

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
        if (obj is not ulong channelId)
        {
            throw new ArgumentException("Object is not ulong", nameof(obj));
        }

        if (context is InteractionActionContext interaction)
        {
            DiscordChannel channel
                = interaction.Interaction.Data.Resolved.Channels[channelId];
            return channel;
        }

        return await context.Client.GetChannelAsync(channelId);
    }

    public async ValueTask<object?> ConvertFromString(Optional<string> input,
        IActionContext context)
    {
        if (!input.TryGetValue(out string? value))
        {
            throw new ArgumentNullException(nameof(input));
        }

        if (ulong.TryParse(value, out ulong channelId))
        {
            return await context.Client.GetChannelAsync(channelId);
        }

        Regex channelMention = ChannelMentionRegex();
        Match match = channelMention.Match(value);
        if (match.Success)
        {
            channelId = ulong.Parse(match.Groups[1].Value);
            return await context.Client.GetChannelAsync(channelId);
        }
        else
        {
            throw new ArgumentException("Input does not follow channel format", nameof(input));
        }
    }
}
