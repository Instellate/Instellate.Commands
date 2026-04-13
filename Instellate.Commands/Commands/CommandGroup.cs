using System.Reflection;
using DSharpPlus.Entities;
using Instellate.Commands.Attributes;
using Instellate.Commands.Attributes.Application;

namespace Instellate.Commands.Commands;

/// <summary>
/// A command group.
/// Command groups handles grouping of multiple commands under a category
/// </summary>
public class CommandGroup : ICommand
{
    internal readonly Dictionary<string, ICommand> _children = new();

    /// <inheritdoc/>
    public string Name { get; }

    /// <inheritdoc/>
    public string Description { get; }

    /// <summary>
    /// The children of this command group
    /// </summary>
    public IReadOnlyDictionary<string, ICommand> Children => this._children;

    /// <summary>
    /// Required permissions to execute commands in this command group
    /// </summary>
    public DiscordPermissions? RequirePermissions { get; }

    /// <summary>
    /// Integration type for this command group
    /// </summary>
    public IReadOnlyList<DiscordApplicationIntegrationType>? IntegrationTypes { get; }

    /// <summary>
    /// Context types for this command group
    /// </summary>
    public IReadOnlyList<DiscordInteractionContextType>? Contexts { get; }

    /// <summary>
    /// If commands in this command group requires being executed in a guild or not
    /// </summary>
    public bool RequireGuild { get; }

    internal CommandGroup(
        string name,
        string description,
        Type type
    )
    {
        this.Name = name;
        this.Description = description;
        this.RequirePermissions
            = type.GetCustomAttribute<RequirePermissionsAttribute>()?.Permissions;
        this.IntegrationTypes
            = type.GetCustomAttribute<AppIntegrationAttribute>()?.IntegrationTypes;
        this.Contexts
            = type.GetCustomAttribute<AppContextsAttribute>()?.Contexts;
        this.RequireGuild = type.GetCustomAttribute<RequireGuildAttribute>() is not null;
    }

    /// <inheritdoc/>
    public DiscordApplicationCommand ConstructApplicationCommand()
    {
        List<DiscordApplicationCommandOption> children = new(this.Children.Count);
        foreach (ICommand child in this.Children.Values)
        {
            children.Add(child.ConstructApplicationCommandOption());
        }

        return new DiscordApplicationCommand(
            this.Name,
            this.Description,
            children,
            allowDMUsage: !RequireGuild,
            defaultMemberPermissions: this.RequirePermissions,
            integrationTypes: this.IntegrationTypes,
            contexts: this.Contexts
        );
    }

    /// <inheritdoc/>
    public DiscordApplicationCommandOption ConstructApplicationCommandOption()
    {
        List<DiscordApplicationCommandOption> children = new(this.Children.Count);
        foreach (ICommand child in this.Children.Values)
        {
            children.Add(child.ConstructApplicationCommandOption());
        }

        return new DiscordApplicationCommandOption(
            this.Name,
            this.Description,
            DiscordApplicationCommandOptionType.SubCommand,
            options: children
        );
    }
}
