namespace Shipping.Domain;

public record ShippingCarrierEnabledEvent(
    Guid CarrierId,
    string Name,
    ShippingProvider Provider) : DomainEvent;

public record ShippingCarrierDisabledEvent(
    Guid CarrierId,
    string Name,
    ShippingProvider Provider) : DomainEvent;

public record ShippingCarrierConfigurationUpdatedEvent(
    Guid CarrierId,
    string Name) : DomainEvent;
