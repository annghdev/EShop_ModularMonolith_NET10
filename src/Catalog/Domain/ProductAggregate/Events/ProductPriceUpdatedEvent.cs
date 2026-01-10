namespace Catalog.Domain;

public record ProductPriceUpdatedEvent(Guid ProductId, decimal Cost, decimal Price) : DomainEvent;
