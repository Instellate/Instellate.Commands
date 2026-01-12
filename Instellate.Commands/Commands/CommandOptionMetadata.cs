namespace Instellate.Commands.Commands;

public class CommandOptionMetadata
{
    public string Name { get; }
    public string Description { get; }
    public bool Optional { get; }
    public int? Positional { get; }
    public object? MinValue { get; internal set; } = null;
    public object? MaxValue { get; internal set; } = null;

    public CommandOptionMetadata(string name, string description, bool optional, int? positional)
    {
        this.Name = name;
        this.Description = description;
        this.Optional = optional;
        this.Positional = positional;
    }
}
