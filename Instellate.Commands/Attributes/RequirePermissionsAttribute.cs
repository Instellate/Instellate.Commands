using DSharpPlus.Entities;

namespace Instellate.Commands.Attributes;

public class RequirePermissionsAttribute : Attribute
{
    public DiscordPermissions Permissions { get; }

    public RequirePermissionsAttribute(params DiscordPermission[] permissions)
    {
        this.Permissions = new DiscordPermissions(permissions);
    }
}
