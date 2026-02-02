namespace Payment.Domain;

public class PaymentTransaction : AggregateRoot
{
    public string TransactionNumber { get; private set; } = string.Empty;
    public Guid OrderId { get; private set; }
    public string OrderNumber { get; private set; } = string.Empty;
    public Guid? GatewayId { get; private set; }
    public PaymentGateway? Gateway { get; private set; }
    public PaymentProvider Provider { get; private set; }
    public Money Amount { get; private set; }
    public PaymentTransactionStatus Status { get; private set; }
    public Guid CustomerId { get; private set; }
    public string? Description { get; private set; }
    public DateTimeOffset InitiatedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public DateTimeOffset? ExpiresAt { get; private set; }
    public PaymentMetadata Metadata { get; private set; }

    private readonly List<PaymentRefund> _refunds = [];
    public IReadOnlyCollection<PaymentRefund> Refunds => _refunds.AsReadOnly();

    public Money TotalRefundedAmount => new(_refunds
        .Where(r => r.Status == RefundStatus.Completed)
        .Sum(r => r.RefundAmount.Amount), Amount.Currency);

    public bool CanRefund => Status == PaymentTransactionStatus.Success &&
                             _refunds.All(r => r.Status != RefundStatus.Processing);

    private PaymentTransaction() { } // EF Core

    public static PaymentTransaction InitiatePayment(
        Guid orderId,
        string orderNumber,
        Guid customerId,
        Money amount,
        PaymentProvider provider,
        Guid? gatewayId = null,
        string? description = null,
        int expirationMinutes = 15)
    {
        if (orderId == Guid.Empty)
            throw new DomainException("Order ID is required");

        if (string.IsNullOrWhiteSpace(orderNumber))
            throw new DomainException("Order number is required");

        if (customerId == Guid.Empty)
            throw new DomainException("Customer ID is required");

        if (amount == null || amount.Amount <= 0)
            throw new DomainException("Amount must be greater than zero");

        if (provider != PaymentProvider.COD && gatewayId == null)
            throw new DomainException("Gateway ID is required for non-COD payments");

        var transaction = new PaymentTransaction
        {
            TransactionNumber = GenerateTransactionNumber(),
            OrderId = orderId,
            OrderNumber = orderNumber,
            CustomerId = customerId,
            Amount = amount,
            Provider = provider,
            GatewayId = gatewayId,
            Description = description,
            Status = PaymentTransactionStatus.Pending,
            InitiatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(expirationMinutes),
            Metadata = new PaymentMetadata()
        };

        transaction.AddEvent(new PaymentInitiatedEvent(
            transaction.Id,
            transaction.TransactionNumber,
            orderId,
            orderNumber,
            amount,
            provider,
            customerId));

        return transaction;
    }

    private static string GenerateTransactionNumber()
    {
        var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
        var random = RandomCodeGenerator.Generate(6);
        return $"PAY-{timestamp}-{random}";
    }

    public void StartProcessing()
    {
        if (Status != PaymentTransactionStatus.Pending)
            throw new DomainException("Can only start processing from Pending status");

        if (ExpiresAt.HasValue && DateTimeOffset.UtcNow > ExpiresAt.Value)
        {
            MarkExpired();
            throw new DomainException("Payment transaction has expired");
        }

        Status = PaymentTransactionStatus.Processing;
        IncreaseVersion();
    }

    public void MarkSuccess(
        string? externalTransactionId = null,
        string? bankCode = null,
        string? cardType = null,
        string? responseCode = null,
        string? responseMessage = null)
    {
        if (Status != PaymentTransactionStatus.Processing && Status != PaymentTransactionStatus.Pending)
            throw new DomainException("Can only mark success from Processing or Pending status");

        Status = PaymentTransactionStatus.Success;
        CompletedAt = DateTimeOffset.UtcNow;

        // Update metadata
        Metadata = Metadata.WithExternalTransactionId(externalTransactionId ?? Metadata.ExternalTransactionId ?? string.Empty);

        if (!string.IsNullOrWhiteSpace(responseCode))
            Metadata = Metadata.WithResponseData(responseCode, responseMessage ?? string.Empty);

        if (!string.IsNullOrWhiteSpace(bankCode))
            Metadata = Metadata.WithBankInfo(bankCode, cardType);

        AddEvent(new PaymentCompletedEvent(
            Id,
            TransactionNumber,
            OrderId,
            OrderNumber,
            Amount,
            Provider,
            CustomerId,
            externalTransactionId));

        IncreaseVersion();
    }

    public void MarkFailed(string? responseCode = null, string? responseMessage = null)
    {
        if (Status == PaymentTransactionStatus.Success)
            throw new DomainException("Cannot mark successful payment as failed");

        if (Status == PaymentTransactionStatus.Refunded)
            throw new DomainException("Cannot mark refunded payment as failed");

        Status = PaymentTransactionStatus.Failed;
        CompletedAt = DateTimeOffset.UtcNow;

        if (!string.IsNullOrWhiteSpace(responseCode))
            Metadata = Metadata.WithResponseData(responseCode, responseMessage ?? string.Empty);

        AddEvent(new PaymentFailedEvent(
            Id,
            TransactionNumber,
            OrderId,
            OrderNumber,
            Amount,
            Provider,
            responseCode,
            responseMessage));

        IncreaseVersion();
    }

    public void Cancel(string reason)
    {
        if (Status == PaymentTransactionStatus.Success)
            throw new DomainException("Cannot cancel successful payment");

        if (Status == PaymentTransactionStatus.Refunded)
            throw new DomainException("Cannot cancel refunded payment");

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Cancellation reason is required");

        Status = PaymentTransactionStatus.Cancelled;

        AddEvent(new PaymentCancelledEvent(Id, TransactionNumber, OrderId, reason));
        IncreaseVersion();
    }

    public void MarkExpired()
    {
        if (Status != PaymentTransactionStatus.Pending && Status != PaymentTransactionStatus.Processing)
            throw new DomainException("Can only mark expired from Pending or Processing status");

        Status = PaymentTransactionStatus.Expired;
        IncreaseVersion();
    }

    public void InitiateRefund(Money refundAmount, string reason, Guid? refundedBy = null)
    {
        if (!CanRefund)
            throw new DomainException("Payment cannot be refunded at this time");

        if (refundAmount.Amount > (Amount.Amount - TotalRefundedAmount.Amount))
            throw new DomainException("Refund amount exceeds available amount");

        var refund = new PaymentRefund(Id, refundAmount, reason, refundedBy);
        _refunds.Add(refund);

        if (Status == PaymentTransactionStatus.Success)
            Status = PaymentTransactionStatus.Refunding;

        AddEvent(new RefundInitiatedEvent(
            refund.Id,
            Id,
            TransactionNumber,
            OrderId,
            refundAmount,
            reason,
            refundedBy));

        IncreaseVersion();
    }

    public void CompleteRefund(Guid refundId)
    {
        var refund = _refunds.FirstOrDefault(r => r.Id == refundId)
            ?? throw new DomainException("Refund not found");

        refund.Complete();

        // Check if fully refunded
        if (TotalRefundedAmount.Amount >= Amount.Amount)
        {
            Status = PaymentTransactionStatus.Refunded;
        }

        AddEvent(new RefundCompletedEvent(
            refund.Id,
            Id,
            TransactionNumber,
            OrderId,
            refund.RefundAmount));

        IncreaseVersion();
    }

    public void FailRefund(Guid refundId)
    {
        var refund = _refunds.FirstOrDefault(r => r.Id == refundId)
            ?? throw new DomainException("Refund not found");

        refund.MarkFailed();

        // If no more pending refunds, revert status to Success
        if (_refunds.All(r => r.Status != RefundStatus.Processing && r.Status != RefundStatus.Pending))
        {
            Status = PaymentTransactionStatus.Success;
        }

        IncreaseVersion();
    }

    public void UpdateMetadata(PaymentMetadata metadata)
    {
        Metadata = metadata ?? throw new DomainException("Metadata cannot be null");
        IncreaseVersion();
    }
}
