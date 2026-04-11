using JetBrains.Annotations;

namespace Instellate.Commands.Attributes;

/// <summary>
/// Specifies that a method is suppose to be used for a command
/// </summary>
[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class CommandAttribute : Attribute
{
    /// <summary>
    /// The name for the command
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 
    /// The descripton for the command
    /// </summary>
    public string Description { get; }

    /// <summary></summary>
    /// <param name="name">The name for the command</param>
    /// <param name="description">The description for the command</param>
    public CommandAttribute(string name, string description)
    {
        this.Name = name;
        this.Description = description;
    }
}
