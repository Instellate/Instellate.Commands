using DSharpPlus.Entities;

namespace Instellate.Commands.Attributes.Application;

/// <summary>
/// App context types that a command runs as
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AppContextsAttribute : Attribute
{
    public IReadOnlyList<DiscordInteractionContextType> Contexts { get; }

    public AppContextsAttribute(params DiscordInteractionContextType[] contexts)
    {
        this.Contexts = contexts;
    }
}
