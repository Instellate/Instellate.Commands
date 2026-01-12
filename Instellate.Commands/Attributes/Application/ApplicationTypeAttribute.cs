using DSharpPlus.Entities;

namespace Instellate.Commands.Attributes.Application;

[AttributeUsage(AttributeTargets.Method)]
public class ApplicationTypeAttribute : Attribute
{
    public DiscordApplicationCommandType Type { get; }

    public ApplicationTypeAttribute(DiscordApplicationCommandType type)
    {
        this.Type = type;
    }
}
