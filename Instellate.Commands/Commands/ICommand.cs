using DSharpPlus.Entities;

namespace Instellate.Commands.Commands;

public interface ICommand
{
    public string Name { get; }

    public DiscordApplicationCommand ConstructApplicationCommand();

    public DiscordApplicationCommandOption ConstructApplicationCommandOption();
}
