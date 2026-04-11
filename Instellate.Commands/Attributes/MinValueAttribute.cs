namespace Instellate.Commands.Attributes;

/// <summary>
/// The min value for a command
/// </summary>
/// <remarks>For parameters that accept numbers as values</remarks>
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
