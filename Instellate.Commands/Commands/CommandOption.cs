using DSharpPlus.Entities;

namespace Instellate.Commands.Commands;

/// <summary>
/// A command option, represents a parameter value most of the time
/// </summary>
public class CommandOption
{
    /// <summary>
    /// The command option name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The command option description
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The command option type when registered as a application command
    /// </summary>
    public DiscordApplicationCommandOptionType Type { get; set; }

    /// <summary>
    /// The possible maximum value that this option supports
    /// </summary>
    public object? MaxValue { get; set; }

    /// <summary>
    /// The possible minimum value that this option supports
    /// </summary>
    public object? MinValue { get; set; }

    /// <summary>
    /// If this option is optional or not
    /// </summary>
    public bool Optional { get; set; }

    /// <summary>
    /// If this option is positional or not in a text context
    /// </summary>
    public int? Positional { get; set; }

    /// <summary>
    /// Possible choices that the command can be
    /// </summary>
    public IReadOnlyList<DiscordApplicationCommandOptionChoice> Choices { get; set; } = [];

    // These should always be set

    /// <summary>
    /// The converter used to convert input to the command options actual type
    /// </summary>
    public Type ConverterType { get; internal set; } = null!;

    /// <summary>
    /// The type value that the command option accepts
    /// </summary>
    public Type ParameterType { get; internal set; } = null!;

    /// <summary>
    /// The default value of the command option
    /// </summary>
    public object? DefaultValue { get; internal set; }


    public CommandOption(string name, string description, bool optional, int? positional)
    {
        this.Name = name;
        this.Description = description;
        this.Optional = optional;
        this.Positional = positional;
    }

    /// <summary>
    /// Constructs a application coammand option entity from the defined metadata
    /// </summary>
    /// <returns></returns>
    public DiscordApplicationCommandOption ConstructEntity()
    {
        return new DiscordApplicationCommandOption(
            this.Name,
            this.Description,
            this.Type,
            !this.Optional,
            minValue: this.MinValue!,
            maxValue: this.MaxValue!,
            choices: this.Choices
        );
    }
}
