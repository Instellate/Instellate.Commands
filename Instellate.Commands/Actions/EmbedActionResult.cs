using DSharpPlus.Entities;

namespace Instellate.Commands.Actions;

public sealed class EmbedActionResult : IActionResult
{
    private readonly DiscordEmbedBuilder _builder;

    public EmbedActionResult()
    {
        this._builder = new DiscordEmbedBuilder();
    }

    public EmbedActionResult(DiscordEmbedBuilder builder)
    {
        this._builder = builder;
    }

    public EmbedActionResult AddField(string name, string value, bool inline)
    {
        this._builder.AddField(name, value, inline);
        return this;
    }

    public EmbedActionResult ClearFields()
    {
        this._builder.ClearFields();
        return this;
    }

    public EmbedActionResult RemoveFieldAt(int index)
    {
        this._builder.RemoveFieldAt(index);
        return this;
    }

    public EmbedActionResult RemoveFieldRange(int index, int count)
    {
        this._builder.RemoveFieldRange(index, count);
        return this;
    }

    public EmbedActionResult WithAuthor(string? name, string? url, string? iconUrl)
    {
        this._builder.WithAuthor(name, url, iconUrl);
        return this;
    }

    public EmbedActionResult WithColor(DiscordColor color)
    {
        this._builder.WithColor(color);
        return this;
    }

    public EmbedActionResult WithDescription(string description)
    {
        this._builder.WithDescription(description);
        return this;
    }

    public EmbedActionResult WithFooter(string? text, string? iconUrl)
    {
        this._builder.WithFooter(text, iconUrl);
        return this;
    }

    public EmbedActionResult WithImaegUrl(string url)
    {
        this._builder.WithImageUrl(url);
        return this;
    }

    public EmbedActionResult WithImageUrl(Uri uri)
    {
        this._builder.WithImageUrl(uri);
        return this;
    }

    public EmbedActionResult WithThumbnail(string url, int height = 0, int width = 0)
    {
        this._builder.WithThumbnail(url, height, width);
        return this;
    }

    public EmbedActionResult WithThumbnail(Uri uri, int height = 0, int width = 0)
    {
        this._builder.WithThumbnail(uri, height, width);
        return this;
    }

    public EmbedActionResult WithTimestamp(DateTimeOffset? timestamp)
    {
        this._builder.WithTimestamp(timestamp);
        return this;
    }

    public EmbedActionResult WithTimestamp(DateTime? timestamp)
    {
        this._builder.WithTimestamp(timestamp);
        return this;
    }

    public EmbedActionResult WithTimestamp(ulong timestamp)
    {
        this._builder.WithTimestamp(timestamp);
        return this;
    }

    public EmbedActionResult WithTitle(string title)
    {
        this._builder.WithTitle(title);
        return this;
    }

    public EmbedActionResult WithUrl(string url)
    {
        this._builder.WithUrl(url);
        return this;
    }

    public EmbedActionResult WithUrl(Uri uri)
    {
        this._builder.WithUrl(uri);
        return this;
    }

    public Task ExecuteResultAsync(IActionContext context)
    {
        DiscordMessageBuilder builder = new();
        builder.AddEmbed(this._builder.Build());
        return context.CreateResponseAsync(builder);
    }
}
