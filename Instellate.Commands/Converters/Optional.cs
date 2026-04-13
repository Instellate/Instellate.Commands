using System.Diagnostics.CodeAnalysis;

namespace Instellate.Commands.Converters;

/// <summary>
/// Struct to represent JSON like values.
/// JSON values might be present but null, or might not be present.
/// This struct can represent such states.
/// </summary>
/// <typeparam name="T"></typeparam>
public readonly struct Optional<T>
{
    /// <summary>
    /// The value is present
    /// </summary>
    public bool IsPresent { get; }

    /// <summary>
    /// The value is present and is not null
    /// </summary>
    public bool HasValue => this.Value is not null;

    /// <summary>
    /// The value
    /// </summary>
    public T? Value { get; }

    public Optional(T? value, bool isPresent)
    {
        this.Value = value;
        this.IsPresent = isPresent;
    }

    public Optional(T value)
    {
        this.Value = value;
        this.IsPresent = true;
    }

    public Optional(bool isPresent)
    {
        this.IsPresent = isPresent;
    }

    public bool TryGetValue([MaybeNullWhen(false)] out T value)
    {
        if (this.Value is { } v)
        {
            value = v;
            return true;
        }

        value = default;
        return false;
    }

    public override string? ToString()
    {
        return this.Value?.ToString();
    }
}
