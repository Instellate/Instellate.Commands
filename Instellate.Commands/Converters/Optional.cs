using System.Diagnostics.CodeAnalysis;

namespace Instellate.Commands.Converters;

public readonly struct Optional<T>
{
    public bool IsPresent { get; }
    public bool HasValue => this.Value is not null;
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
        else
        {
            value = default;
            return false;
        }
    }

    public override string? ToString()
    {
        return Value?.ToString();
    }
}
