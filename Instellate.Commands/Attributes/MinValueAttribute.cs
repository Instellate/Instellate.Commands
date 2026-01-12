namespace Instellate.Commands.Attributes;

[AttributeUsage(AttributeTargets.Parameter)]
public class MinValueAttribute : Attribute
{
    public object Value { get; }

    public MinValueAttribute(int value)
    {
        this.Value = value;
    }

    public MinValueAttribute(long value)
    {
        this.Value = value;
    }

    public MinValueAttribute(ulong value)
    {
        this.Value = value;
    }

    public MinValueAttribute(double value)
    {
        this.Value = value;
    }

    public MinValueAttribute(float value)
    {
        this.Value = value;
    }
}
