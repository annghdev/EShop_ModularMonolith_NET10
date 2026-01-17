namespace Inventory.Domain;

/// <summary>
/// Represents a reservation of inventory for a specific order
/// </summary>
public class InventoryReservation : BaseEntity
{
    public Guid InventoryItemId { get; private set; }
    public InventoryItem? InventoryItem { get; private set; }
    public Guid OrderId { get; private set; }
    public int Quantity { get; private set; }
    public DateTimeOffset? ExpiresAt { get; private set; }

    private InventoryReservation() { } // EF Core

    internal InventoryReservation(Guid inventoryItemId, Guid orderId, int quantity, DateTimeOffset? expiresAt = null)
    {
        if (orderId == Guid.Empty)
            throw new DomainException("Order ID cannot be empty");

        if (quantity <= 0)
            throw new DomainException("Reservation quantity must be greater than 0");

        InventoryItemId = inventoryItemId;
        OrderId = orderId;
        Quantity = quantity;
        ExpiresAt = expiresAt;
    }

    internal void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new DomainException("Reservation quantity must be greater than 0");

        Quantity = newQuantity;
    }

    public bool IsExpired() => ExpiresAt.HasValue && ExpiresAt.Value < DateTimeOffset.UtcNow;
}
