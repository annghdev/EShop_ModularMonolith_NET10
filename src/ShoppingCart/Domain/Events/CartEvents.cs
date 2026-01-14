namespace ShoppingCart.Domain.Events;

// Item events
public record CartItemAddedEvent(
    Guid CartId,
    Guid VariantId,
    string Sku,
    int Quantity) : DomainEvent;

public record CartItemQuantityUpdatedEvent(
    Guid CartId,
    Guid VariantId,
    int NewQuantity) : DomainEvent;

public record CartItemRemovedEvent(
    Guid CartId,
    Guid VariantId) : DomainEvent;

public record CartClearedEvent(Guid CartId) : DomainEvent;

// Coupon events
public record CouponAppliedToCartEvent(
    Guid CartId,
    string CouponCode,
    Guid CouponId) : DomainEvent;

public record CouponRemovedFromCartEvent(
    Guid CartId,
    string CouponCode) : DomainEvent;

// Lifecycle events
public record CartCheckedOutEvent(
    Guid CartId,
    Guid OrderId,
    Guid? CustomerId,
    string? GuestId) : DomainEvent;

public record CartMergedEvent(
    Guid TargetCartId,
    Guid SourceCartId) : DomainEvent;

public record CartAbandonedEvent(
    Guid CartId,
    Guid? CustomerId,
    string? GuestId) : DomainEvent;
