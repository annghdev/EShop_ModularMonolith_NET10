namespace Payment.Domain;

public record PaymentCancelledEvent(
    Guid TransactionId,
    string TransactionNumber,
    Guid OrderId,
    string Reason) : DomainEvent;

public record PaymentCompletedEvent(
    Guid TransactionId,
    string TransactionNumber,
    Guid OrderId,
    string OrderNumber,
    Money Amount,
    PaymentProvider Provider,
    Guid CustomerId,
    string? ExternalTransactionId) : DomainEvent;

public record PaymentFailedEvent(
    Guid TransactionId,
    string TransactionNumber,
    Guid OrderId,
    string OrderNumber,
    Money Amount,
    PaymentProvider Provider,
    string? ResponseCode,
    string? ResponseMessage) : DomainEvent;

public record PaymentInitiatedEvent(
    Guid TransactionId,
    string TransactionNumber,
    Guid OrderId,
    string OrderNumber,
    Money Amount,
    PaymentProvider Provider,
    Guid CustomerId) : DomainEvent;

public record RefundCompletedEvent(
    Guid RefundId,
    Guid TransactionId,
    string TransactionNumber,
    Guid OrderId,
    Money RefundAmount) : DomainEvent;

public record RefundInitiatedEvent(
    Guid RefundId,
    Guid TransactionId,
    string TransactionNumber,
    Guid OrderId,
    Money RefundAmount,
    string Reason,
    Guid? RefundedBy) : DomainEvent;
