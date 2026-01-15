using DSharpPlus.Entities;

namespace Instellate.Commands.Commands;

public class CommandGroup : ICommand
{
    internal readonly Dictionary<string, ICommand> _children = new();

    public string Name { get; }
    public string Description { get; }
    public DiscordPermissions? RequirePermissions { get; }
    public IReadOnlyDictionary<string, ICommand> Children => this._children;
    public IReadOnlyList<DiscordApplicationIntegrationType>? IntegrationTypes { get; }

    internal CommandGroup(
        string name,
        string description,
        DiscordPermissions? requirePermissions,
        IReadOnlyList<DiscordApplicationIntegrationType>? integrationTypes
    )
    {
        this.Name = name;
        this.Description = description;
        this.RequirePermissions = requirePermissions;
        this.IntegrationTypes = integrationTypes;
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
