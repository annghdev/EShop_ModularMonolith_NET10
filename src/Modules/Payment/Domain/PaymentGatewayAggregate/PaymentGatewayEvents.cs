namespace Payment.Domain;

public record PaymentGatewayConfigurationUpdatedEvent(
    Guid GatewayId,
    string Name) : DomainEvent;

public record PaymentGatewayDisabledEvent(
    Guid GatewayId,
    string Name,
    PaymentProvider Provider) : DomainEvent;

public record PaymentGatewayEnabledEvent(
    Guid GatewayId,
    string Name,
    PaymentProvider Provider) : DomainEvent;
