namespace Instellate.Commands;

public class ComponentInteractionResult
{
    public string CustomId { get; }
    public string[]? Values { get; }

    public ComponentInteractionResult(string customId, string[]? values)
    {
        this.CustomId = customId;
        this.Values = values;
    }
}
