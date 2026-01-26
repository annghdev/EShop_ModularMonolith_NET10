namespace Pricing.Domain;

public record ProductCostUpdatedEvent(
    Guid ProductId,
    Guid? VariantId,
    string Sku,
    Money NewCost) : DomainEvent;

public record ProductPriceUpdatedEvent(
    Guid ProductId,
    Guid? VariantId,
    string Sku,
    Money NewPrice) : DomainEvent;