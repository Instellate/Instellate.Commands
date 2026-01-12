namespace Instellate.Commands.Attributes.Text;

[AttributeUsage(AttributeTargets.Parameter)]
public class PositionalAttribute : Attribute
{
    public int Index { get; }

    public PositionalAttribute(int index)
    {
        this.Index = index;
    }
}
