namespace Inventory.Domain;

/// <summary>
/// Type of inventory movement for audit logging
/// </summary>
public enum MovementType
{
    /// <summary>
    /// Goods received into warehouse (purchase order, return, etc.)
    /// </summary>
    Received,

    /// <summary>
    /// Goods shipped out of warehouse (direct shipment without order)
    /// </summary>
    Shipped,

    /// <summary>
    /// Quantity reserved for an order
    /// </summary>
    Reserved,

    /// <summary>
    /// Reservation released (order cancelled before confirmation)
    /// </summary>
    Released,

    /// <summary>
    /// Reservation confirmed and stock deducted (order fulfilled)
    /// </summary>
    Confirmed,

    /// <summary>
    /// Manual inventory adjustment (stocktake, damage, etc.)
    /// </summary>
    Adjusted,

    /// <summary>
    /// Inter-warehouse transfer
    /// </summary>
    Transferred
}
