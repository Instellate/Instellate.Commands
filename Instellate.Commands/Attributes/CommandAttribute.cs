namespace Instellate.Commands.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class CommandAttribute : Attribute
{
    public string Name { get; }
    public string Description { get; }
    
    public CommandAttribute(string name, string description)
    {
        this.Name = name;
        this.Description = description;
    }
}
