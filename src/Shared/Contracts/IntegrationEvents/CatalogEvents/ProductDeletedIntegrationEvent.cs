namespace Contracts.IntegrationEvents.CatalogEvents;

public record ProductDeletedIntegrationEvent(Guid ProductId) : IntegrationEvent;