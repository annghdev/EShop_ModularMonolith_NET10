namespace Catalog.Domain;

public record ProductThumbnailUpdatedEvent(Guid ProductId, string ImageUrl) : DomainEvent;
