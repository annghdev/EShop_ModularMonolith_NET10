namespace Shipping.Domain;

public record ShipmentCreatedEvent(
    Guid ShipmentId,
    string ShipmentNumber,
    Guid OrderId,
    string OrderNumber,
    ShippingProvider Provider) : DomainEvent;

public record ShipmentPickedUpEvent(
    Guid ShipmentId,
    string ShipmentNumber,
    Guid OrderId,
    string? TrackingNumber) : DomainEvent;

public record ShipmentInTransitEvent(
    Guid ShipmentId,
    string ShipmentNumber,
    Guid OrderId,
    string? Location) : DomainEvent;

public record ShipmentOutForDeliveryEvent(
    Guid ShipmentId,
    string ShipmentNumber,
    Guid OrderId) : DomainEvent;

public record ShipmentDeliveredEvent(
    Guid ShipmentId,
    string ShipmentNumber,
    Guid OrderId,
    string OrderNumber,
    DateTimeOffset DeliveredAt) : DomainEvent;

public record ShipmentDeliveryFailedEvent(
    Guid ShipmentId,
    string ShipmentNumber,
    Guid OrderId,
    string Reason) : DomainEvent;

public record ShipmentReturnedEvent(
    Guid ShipmentId,
    string ShipmentNumber,
    Guid OrderId,
    string? Reason) : DomainEvent;

public record ShipmentCancelledEvent(
    Guid ShipmentId,
    string ShipmentNumber,
    Guid OrderId,
    string Reason) : DomainEvent;
