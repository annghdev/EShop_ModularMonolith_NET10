namespace Catalog.Domain;

public record ProductPriceUpdatedEvent(Product Payload) : DomainEvent;
