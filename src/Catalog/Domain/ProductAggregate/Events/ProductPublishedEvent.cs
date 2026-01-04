namespace Catalog.Domain;

public record ProductPublishedEvent(Product Payload) : DomainEvent;
public record ProductDiscontinuedEvent(Guid ProductId) : DomainEvent;
