namespace Catalog.Domain;

public record VariantAddedEvent(Variant Payload) : DomainEvent;
public record VariantRemovedEvent(Variant Payload) : DomainEvent;