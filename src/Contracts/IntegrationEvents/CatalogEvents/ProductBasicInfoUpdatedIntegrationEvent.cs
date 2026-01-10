namespace Contracts.IntegrationEvents;

public record ProductBasicInfoUpdatedIntegrationEvent(Guid ProductId) : IntegrationEvent;