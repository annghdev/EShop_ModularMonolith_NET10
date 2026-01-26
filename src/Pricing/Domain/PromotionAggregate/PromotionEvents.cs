namespace Pricing.Domain;

public record PromotionActivatedEvent(
    Guid PromotionId,
    string Name,
    PromotionType Type) : DomainEvent;

public record PromotionDeactivatedEvent(
    Guid PromotionId,
    string Name) : DomainEvent;

public record PromotionEndedEvent(
    Guid PromotionId,
    string Name) : DomainEvent;
