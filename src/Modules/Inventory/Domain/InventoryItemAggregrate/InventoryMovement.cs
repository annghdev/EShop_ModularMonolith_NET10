namespace Inventory.Domain;

/// <summary>
/// Audit log of all inventory movements
/// </summary>
public class InventoryMovement : BaseEntity
{
    public Guid InventoryItemId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid VariantId { get; private set; }
    public Guid? OrderId { get; private set; }
    public int Quantity { get; private set; }
    public MovementType Type { get; private set; }
    public int SnapshotQuantity { get; private set; }
    public string? Reference { get; private set; }
    public string? Notes { get; private set; }

    private InventoryMovement() { } // EF Core

    public static InventoryMovement Create(
        Guid inventoryItemId,
        Guid warehouseId,
        Guid productId,
        Guid variantId,
        int quantity,
        MovementType type,
        int snapshotQuantity,
        Guid? orderId = null,
        string? reference = null,
        string? notes = null)
    {
        return new InventoryMovement
        {
            InventoryItemId = inventoryItemId,
            WarehouseId = warehouseId,
            ProductId = productId,
            VariantId = variantId,
            Quantity = quantity,
            Type = type,
            SnapshotQuantity = snapshotQuantity,
            OrderId = orderId,
            Reference = reference,
            Notes = notes
        };
    }
}
