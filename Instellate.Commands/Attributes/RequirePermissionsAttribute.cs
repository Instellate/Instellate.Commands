using DSharpPlus.Entities;

namespace Instellate.Commands.Attributes;

/// <summary>
/// The command requires certain permissions to be executed
/// </summary>
public class RequirePermissionsAttribute : Attribute
{
    /// <summary>
    /// The permissions required
    /// </summary>
    public DiscordPermissions Permissions { get; }

    /// <summary>
    /// </summary>
    /// <param name="permissions">The permissions required</param>
    public RequirePermissionsAttribute(params DiscordPermission[] permissions)
    {
        this.Permissions = new DiscordPermissions(permissions);
    }
}
