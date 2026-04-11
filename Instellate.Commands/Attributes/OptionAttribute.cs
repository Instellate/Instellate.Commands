namespace Instellate.Commands.Attributes;

/// <summary>
/// Specifies that a parameter is a discord option
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public class OptionAttribute : Attribute
{
    /// <summary>
    /// The name for the option
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The description for the option
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// </summary>
    /// <param name="name">The name for the option</param>
    /// <param name="description">The description for the option</param>
    public OptionAttribute(string name, string description)
    {
        this.Name = name;
        this.Description = description;
    }
}
