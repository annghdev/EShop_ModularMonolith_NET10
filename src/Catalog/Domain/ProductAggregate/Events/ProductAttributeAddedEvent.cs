namespace Catalog.Domain;

public record ProductAttributeAddedEvent(ProductAttribute Payload) : DomainEvent;
public record ProductAttributeRemovedEvent(ProductAttribute Payload) : DomainEvent;