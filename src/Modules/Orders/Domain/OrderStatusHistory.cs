namespace Orders.Domain;

public class OrderStatusHistory : BaseEntity
{
    public Guid OrderId { get; private set; }
    public Order? Order { get; private set; }
    public OrderStatus FromStatus { get; private set; }
    public OrderStatus ToStatus { get; private set; }
    public string? Reason { get; private set; }
    public Guid? ChangedBy { get; private set; }

    private OrderStatusHistory() { } // EF Core

    internal OrderStatusHistory(
        Guid orderId,
        OrderStatus fromStatus,
        OrderStatus toStatus,
        string? reason = null,
        Guid? changedBy = null)
    {
        OrderId = orderId;
        FromStatus = fromStatus;
        ToStatus = toStatus;
        Reason = reason;
        ChangedBy = changedBy;
    }
}
