namespace Payment.Domain;

public class PaymentRefund : BaseEntity
{
    public Guid PaymentTransactionId { get; private set; }
    public PaymentTransaction? PaymentTransaction { get; private set; }
    public Money RefundAmount { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public RefundStatus Status { get; private set; }
    public DateTimeOffset RefundedAt { get; private set; }
    public Guid? RefundedBy { get; private set; }
    public string? ExternalRefundId { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    private PaymentRefund() { } // EF Core

    internal PaymentRefund(
        Guid paymentTransactionId,
        Money refundAmount,
        string reason,
        Guid? refundedBy = null)
    {
        if (refundAmount == null || refundAmount.Amount <= 0)
            throw new DomainException("Refund amount must be greater than zero");

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Refund reason is required");

        PaymentTransactionId = paymentTransactionId;
        RefundAmount = refundAmount;
        Reason = reason;
        Status = RefundStatus.Pending;
        RefundedAt = DateTimeOffset.UtcNow;
        RefundedBy = refundedBy;
    }

    internal void StartProcessing(string? externalRefundId = null)
    {
        if (Status != RefundStatus.Pending)
            throw new DomainException("Can only start processing from Pending status");

        Status = RefundStatus.Processing;
        ExternalRefundId = externalRefundId;
    }

    internal void Complete()
    {
        if (Status != RefundStatus.Processing)
            throw new DomainException("Can only complete from Processing status");

        Status = RefundStatus.Completed;
        CompletedAt = DateTimeOffset.UtcNow;
    }

    internal void MarkFailed()
    {
        if (Status == RefundStatus.Completed)
            throw new DomainException("Cannot mark completed refund as failed");

        Status = RefundStatus.Failed;
    }
}
