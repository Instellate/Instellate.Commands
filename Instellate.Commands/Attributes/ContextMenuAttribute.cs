using JetBrains.Annotations;

namespace Instellate.Commands.Attributes;

/// <summary>
/// Specifies that a method will be used for a context menu
/// </summary>
[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Method)]
public class ContextMenuAttribute : Attribute
{
    /// <summary>
    /// The name for the context menu
    /// </summary>
    public string Name { get; }

    /// <summary></summary>
    /// <param name="name">The name for the context menu</param>
    public ContextMenuAttribute(string name)
    {
        this.Name = name;
    }
}
