using DSharpPlus.Entities;

namespace Instellate.Commands.Commands;

public class CommandGroup : ICommand
{
    internal readonly Dictionary<string, ICommand> _children = new();

    public string Name { get; }
    public string Description { get; }
    public DiscordPermissions? RequirePermissions { get; }
    public IReadOnlyDictionary<string, ICommand> Children => this._children;

    internal CommandGroup(string name, string description, DiscordPermissions? requirePermissions)
    {
        this.Name = name;
        this.Description = description;
        this.RequirePermissions = requirePermissions;
    }

    public DiscordApplicationCommand ConstructApplicationCommand()
    {
        List<DiscordApplicationCommandOption> children = new(this.Children.Count);
        foreach (ICommand child in this.Children.Values)
        {
            children.Add(child.ConstructApplicationCommandOption());
        }

        return new DiscordApplicationCommand(this.Name, this.Description, children);
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
