namespace Instellate.Commands.Attributes;

/// <summary>
/// Decides the name for a enum choice
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class ChoiceNameAttribute : Attribute
{
    /// <summary>
    /// The name for the command
    /// </summary>
    public string Name { get; }

    public ChoiceNameAttribute(string name)
    {
        this.Name = name;
    }
}
