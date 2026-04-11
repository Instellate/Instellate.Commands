using DSharpPlus.Entities;

namespace Instellate.Commands.Attributes.Application;

/// <summary>
/// App integration types that a command runs as
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class AppIntegrationAttribute : Attribute
{
    public IReadOnlyList<DiscordApplicationIntegrationType>? IntegrationTypes { get; }

    public AppIntegrationAttribute(params DiscordApplicationIntegrationType[] integrations)
    {
        this.IntegrationTypes = integrations.ToList();
    }
}
