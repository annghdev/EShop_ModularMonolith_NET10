namespace Catalog.Domain;

public record ProductImageAddedEvent(Guid ProductId, string ImageUrl) : DomainEvent;
public record ProductImageRemovedEvent(Guid ProductId, string ImageUrl) : DomainEvent;
