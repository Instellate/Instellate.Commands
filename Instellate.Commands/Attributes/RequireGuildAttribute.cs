namespace Instellate.Commands.Attributes;

/// <summary>
/// The command is required to be executed in a guild
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RequireGuildAttribute : Attribute
{
}
