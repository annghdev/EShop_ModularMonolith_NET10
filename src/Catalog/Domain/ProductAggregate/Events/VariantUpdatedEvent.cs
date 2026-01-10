namespace Catalog.Domain;

public record VariantUpdatedEvent(Variant Payload) : DomainEvent;
