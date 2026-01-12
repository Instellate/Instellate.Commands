namespace Instellate.Commands.Attributes;

[AttributeUsage(AttributeTargets.Parameter)]
public class MaxValueAttribute : Attribute
{
    public object Value { get; }

    public MaxValueAttribute(int value)
    {
        this.Value = value;
    }

    public MaxValueAttribute(long value)
    {
        this.Value = value;
    }

    public MaxValueAttribute(ulong value)
    {
        this.Value = value;
    }

    public MaxValueAttribute(double value)
    {
        this.Value = value;
    }

    public MaxValueAttribute(float value)
    {
        this.Value = value;
    }
}
