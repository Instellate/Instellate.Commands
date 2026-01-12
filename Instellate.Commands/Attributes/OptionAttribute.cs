namespace Instellate.Commands.Attributes;

[AttributeUsage(AttributeTargets.Parameter)]
public class OptionAttribute : Attribute
{
    public string Name { get; }
    public string Description { get; }

    public OptionAttribute(string name, string description)
    {
        this.Name = name;
        this.Description = description;
    }
}
