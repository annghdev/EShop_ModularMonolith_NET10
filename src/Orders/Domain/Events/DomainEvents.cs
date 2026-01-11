namespace Orders.Domain;

// Order Lifecycle Events
public record OrderPlacedEvent(
    Guid OrderId,
    string OrderNumber,
    Guid CustomerId,
    IReadOnlyList<OrderItemDto> Items) : DomainEvent;

public record OrderReservationConfirmedEvent(
    Guid OrderId,
    string OrderNumber) : DomainEvent;

public record OrderConfirmedEvent(
    Guid OrderId,
    string OrderNumber) : DomainEvent;

public record OrderShippedEvent(
    Guid OrderId,
    string OrderNumber,
    Guid? ShippingId) : DomainEvent;

public record OrderDeliveredEvent(
    Guid OrderId,
    string OrderNumber) : DomainEvent;

public record OrderCancelledEvent(
    Guid OrderId,
    string OrderNumber,
    string Reason) : DomainEvent;

public record OrderPaidEvent(
    Guid OrderId,
    string OrderNumber,
    Money Amount) : DomainEvent;

public record OrderRefundedEvent(
    Guid OrderId,
    string OrderNumber,
    Money Amount) : DomainEvent;
