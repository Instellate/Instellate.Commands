using System.Reflection;
using DSharpPlus.Entities;
using Instellate.Commands.Attributes;
using Instellate.Commands.Attributes.Application;

namespace Instellate.Commands.Commands;

public class CommandGroup : ICommand
{
    internal readonly Dictionary<string, ICommand> _children = new();

    public string Name { get; }
    public string Description { get; }
    public IReadOnlyDictionary<string, ICommand> Children => this._children;
    public DiscordPermissions? RequirePermissions { get; }
    public IReadOnlyList<DiscordApplicationIntegrationType>? IntegrationTypes { get; }
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
        this.RequireGuild = type.GetCustomAttribute<RequireGuildAttribute>() is not null;
    }

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
            integrationTypes: this.IntegrationTypes
        );
    }

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
