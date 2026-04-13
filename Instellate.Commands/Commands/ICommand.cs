using DSharpPlus.Entities;

namespace Instellate.Commands.Commands;

/// <summary>
/// A interface 
/// </summary>
public interface ICommand
{
    /// <summary>
    /// The name of the command
    /// </summary>
    string Name { get; }

    /// <summary>
    /// The description of the command
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Constructs a application command from the command data
    /// </summary>
    /// <returns></returns>
    DiscordApplicationCommand ConstructApplicationCommand();

    /// <summary>
    /// Constructs a application command option from the command data
    /// </summary>
    /// <returns></returns>
    DiscordApplicationCommandOption ConstructApplicationCommandOption();
}
