namespace Contracts.IntegrationEvents;

public record ProductPricingUpdatedIntegrationEvent(Guid ProductId, decimal Cost, decimal Price) : IntegrationEvent;