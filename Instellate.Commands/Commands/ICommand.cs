using DSharpPlus.Entities;

namespace Instellate.Commands.Commands;

public interface ICommand
{
    public DiscordApplicationCommand ConstructApplicationCommand();

    public DiscordApplicationCommandOption ConstructApplicationCommandOption();
}
