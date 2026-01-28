namespace Catalog.Domain;

public record ProductAttributeAddedEvent(ProductAttribute Payload) : DomainEvent;
public record ProductAttributeRemovedEvent(ProductAttribute Payload) : DomainEvent;

public record ProductBasicInfoUpdatedEvent(Product Payload) : DomainEvent;

public record ProductDeletedEvent(Guid ProductId) : DomainEvent;
public record ProductDraftCreatedEvent(Product Payload) : DomainEvent;

public record ProductThumbnailUpdatedEvent(Guid ProductId, string ImageUrl) : DomainEvent;
public record ProductImageAddedEvent(Guid ProductId, string ImageUrl) : DomainEvent;
public record ProductImageRemovedEvent(Guid ProductId, string ImageUrl) : DomainEvent;

public record ProductPriceUpdatedEvent(Product Payload) : DomainEvent;

public record ProductPublishedEvent(Product Payload) : DomainEvent;
public record ProductDiscontinuedEvent(Guid ProductId) : DomainEvent;
public record ProductRepublishedEvent(Guid ProductId) : DomainEvent;


public record VariantAddedEvent(Variant Payload) : DomainEvent;
public record VariantUpdatedEvent(Variant Payload) : DomainEvent;
public record VariantRemovedEvent(Variant Payload) : DomainEvent;
