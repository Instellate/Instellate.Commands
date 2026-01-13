using JetBrains.Annotations;

namespace Instellate.Commands.Attributes;

[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Method)]
public class ContextMenuAttribute : Attribute
{
    public string Name { get; }

    public ContextMenuAttribute(string name)
    {
        this.Name = name;
    }
}
