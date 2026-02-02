namespace Pricing.Domain;

public record CouponActivatedEvent(
    Guid CouponId,
    string Code) : DomainEvent;

public record CouponDeactivatedEvent(
    Guid CouponId,
    string Code) : DomainEvent;

public record CouponUsedEvent(
    Guid CouponId,
    string Code,
    Guid CustomerId,
    Guid OrderId,
    Money DiscountApplied) : DomainEvent;

public record CouponExhaustedEvent(
    Guid CouponId,
    string Code) : DomainEvent;

public record CouponExpiredEvent(
    Guid CouponId,
    string Code) : DomainEvent;