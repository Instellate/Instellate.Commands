using DSharpPlus.Entities;

namespace Instellate.Commands.Attributes.Application;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AppContextsAttribute : Attribute
{
    public IReadOnlyList<DiscordInteractionContextType> Contexts { get; }

    public AppContextsAttribute(params DiscordInteractionContextType[] contexts)
    {
        this.Contexts = contexts;
    }
}
