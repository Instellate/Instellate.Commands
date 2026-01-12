using System.Reflection;
using DSharpPlus.Entities;

namespace Instellate.Commands.Commands;

public class Command : ICommand
{
    public string Name { get; }
    public string Description { get; }
    public IReadOnlyList<CommandOption> Options { get; }
    internal MethodInfo Method { get; }

    internal Command(string name,
        string description,
        IReadOnlyList<CommandOption> options,
        MethodInfo method)
    {
        this.Name = name;
        this.Description = description;
        this.Options = options;
        this.Method = method;
    }

    public DiscordApplicationCommand ConstructApplicationCommand()
    {
        List<DiscordApplicationCommandOption> options = new(this.Options.Count);
        foreach (CommandOption option in this.Options)
        {
            options.Add(option.ConstructEntity());
        }

        return new DiscordApplicationCommand(this.Name, this.Description, options: options);
    }

    public DiscordApplicationCommandOption ConstructApplicationCommandOption()
    {
        List<DiscordApplicationCommandOption> options = new(this.Options.Count);
        foreach (CommandOption option in this.Options)
        {
            options.Add(option.ConstructEntity());
        }

        return new DiscordApplicationCommandOption(
            this.Name,
            this.Description,
            type: DiscordApplicationCommandOptionType.SubCommand,
            options: options);
    }
}
