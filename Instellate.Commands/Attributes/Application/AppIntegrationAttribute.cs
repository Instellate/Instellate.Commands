using DSharpPlus.Entities;

namespace Instellate.Commands.Attributes.Application;

public class AppIntegrationAttribute : Attribute
{
    public IReadOnlyList<DiscordApplicationIntegrationType> IntegrationTypes { get; }

    public AppIntegrationAttribute(params DiscordApplicationIntegrationType[] integrations)
    {
        this.IntegrationTypes = integrations.ToList();
    }
}
