namespace Instellate.Commands.Commands;

/// <summary>
/// Command option metadata, used by converters to construct application command data
/// </summary>
public class CommandOptionMetadata
{
    /// <summary>
    /// The name of the command option
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The description of the command option
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// If the command option is optional or not
    /// </summary>
    public bool Optional { get; }

    /// <summary>
    /// If the command option is positional or not in a text context
    /// </summary>
    public int? Positional { get; }

    /// <summary>
    /// The possible minimum value that the command option supports
    /// </summary>
    public object? MinValue { get; internal set; } = null;

    /// <summary>
    /// The possible maximum value that the command option supports
    /// </summary>
    public object? MaxValue { get; internal set; } = null;

    public CommandOptionMetadata(string name, string description, bool optional, int? positional)
    {
        this.Name = name;
        this.Description = description;
        this.Optional = optional;
        this.Positional = positional;
    }
}
