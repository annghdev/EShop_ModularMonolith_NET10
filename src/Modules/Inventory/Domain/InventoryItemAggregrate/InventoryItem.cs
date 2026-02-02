namespace Inventory.Domain;

/// <summary>
/// Represents inventory of a specific variant in a specific warehouse.
/// This is an aggregate root to support saga pattern operations.
/// </summary>
public class InventoryItem : AggregateRoot
{
    public Guid WarehouseId { get; private set; }
    public Warehouse? Warehouse { get; private set; }

    // Catalog references
    public Guid ProductId { get; private set; }
    public Guid VariantId { get; private set; }
    public Sku Sku { get; private set; } = null!;

    // Quantity tracking
    public int QuantityOnHand { get; private set; }
    public int LowStockThreshold { get; private set; }

    // Reservations
    private readonly List<InventoryReservation> _reservations = [];
    public IReadOnlyCollection<InventoryReservation> Reservations => _reservations.AsReadOnly();

    // Calculated properties
    public int QuantityReserved => _reservations.Sum(r => r.Quantity);
    public int QuantityAvailable => QuantityOnHand - QuantityReserved;

    private InventoryItem() { } // EF Core

    public static InventoryItem Create(
        Guid warehouseId,
        Guid productId,
        Guid variantId,
        Sku sku,
        int initialQuantity = 0,
        int lowStockThreshold = 5)
    {
        if (warehouseId == Guid.Empty)
            throw new DomainException("Warehouse ID cannot be empty");

        if (productId == Guid.Empty)
            throw new DomainException("Product ID cannot be empty");

        if (variantId == Guid.Empty)
            throw new DomainException("Variant ID cannot be empty");

        return new InventoryItem
        {
            WarehouseId = warehouseId,
            ProductId = productId,
            VariantId = variantId,
            Sku = sku ?? throw new DomainException("SKU cannot be null"),
            QuantityOnHand = initialQuantity,
            LowStockThreshold = lowStockThreshold
        };
    }

    /// <summary>
    /// Receive goods into warehouse
    /// </summary>
    public void Receive(int quantity, string? reference = null)
    {
        if (quantity <= 0)
            throw new DomainException("Receive quantity must be greater than 0");

        var snapshotQuantity = QuantityOnHand;
        QuantityOnHand += quantity;

        AddMovementEvent(quantity, MovementType.Received, snapshotQuantity, null, reference);
        AddEvent(new InventoryReceivedEvent(Id, WarehouseId, ProductId, VariantId, Sku.Value, quantity, QuantityOnHand));
        IncreaseVersion();
    }

    /// <summary>
    /// Ship goods out without order (manual export)
    /// </summary>
    public void Ship(int quantity, string? reference = null)
    {
        if (quantity <= 0)
            throw new DomainException("Ship quantity must be greater than 0");

        if (QuantityAvailable < quantity)
            throw new DomainException($"Insufficient available stock. Available: {QuantityAvailable}, Requested: {quantity}");

        var snapshotQuantity = QuantityOnHand;
        QuantityOnHand -= quantity;

        AddMovementEvent(quantity, MovementType.Shipped, snapshotQuantity, null, reference);
        AddEvent(new InventoryShippedEvent(Id, WarehouseId, ProductId, VariantId, Sku.Value, quantity, QuantityOnHand));
        CheckLowStock();
        IncreaseVersion();
    }

    /// <summary>
    /// Reserve inventory for an order
    /// </summary>
    public void Reserve(Guid orderId, int quantity)
    {
        if (orderId == Guid.Empty)
            throw new DomainException("Order ID cannot be empty");

        if (quantity <= 0)
            throw new DomainException("Reserve quantity must be greater than 0");

        if (QuantityAvailable < quantity)
            throw new DomainException($"Insufficient available stock. Available: {QuantityAvailable}, Requested: {quantity}");

        var snapshotQuantity = QuantityOnHand;

        // Check if already reserved for this order
        var existingReservation = _reservations.FirstOrDefault(r => r.OrderId == orderId);
        if (existingReservation != null)
        {
            existingReservation.UpdateQuantity(quantity);
        }
        else
        {
            var reservation = new InventoryReservation(Id, orderId, quantity);
            _reservations.Add(reservation);
        }

        AddMovementEvent(quantity, MovementType.Reserved, snapshotQuantity, orderId);
        AddEvent(new InventoryReservedEvent(Id, WarehouseId, ProductId, VariantId, orderId, quantity, QuantityAvailable));
        CheckLowStock();
        IncreaseVersion();
    }

    /// <summary>
    /// Release reservation (order cancelled)
    /// </summary>
    public void Release(Guid orderId)
    {
        if (orderId == Guid.Empty)
            throw new DomainException("Order ID cannot be empty");

        var reservation = _reservations.FirstOrDefault(r => r.OrderId == orderId);
        if (reservation == null)
            return; // No reservation to release

        var snapshotQuantity = QuantityOnHand;
        var releasedQuantity = reservation.Quantity;
        _reservations.Remove(reservation);

        AddMovementEvent(releasedQuantity, MovementType.Released, snapshotQuantity, orderId);
        AddEvent(new InventoryReleasedEvent(Id, WarehouseId, ProductId, VariantId, orderId, releasedQuantity, QuantityAvailable));
        IncreaseVersion();
    }

    /// <summary>
    /// Confirm reservation and deduct from on-hand quantity
    /// </summary>
    public void Confirm(Guid orderId)
    {
        if (orderId == Guid.Empty)
            throw new DomainException("Order ID cannot be empty");

        var reservation = _reservations.FirstOrDefault(r => r.OrderId == orderId)
            ?? throw new DomainException($"No reservation found for order {orderId}");

        var snapshotQuantity = QuantityOnHand;
        var confirmedQuantity = reservation.Quantity;

        QuantityOnHand -= confirmedQuantity;
        _reservations.Remove(reservation);

        AddMovementEvent(confirmedQuantity, MovementType.Confirmed, snapshotQuantity, orderId);
        AddEvent(new InventoryConfirmedEvent(Id, WarehouseId, ProductId, VariantId, orderId, confirmedQuantity, QuantityOnHand));
        CheckLowStock();
        IncreaseVersion();
    }

    /// <summary>
    /// Manual inventory adjustment
    /// </summary>
    public void Adjust(int quantityDelta, string reason)
    {
        if (quantityDelta == 0)
            throw new DomainException("Adjustment quantity cannot be zero");

        var newQuantity = QuantityOnHand + quantityDelta;
        if (newQuantity < 0)
            throw new DomainException("Adjustment would result in negative quantity");

        var snapshotQuantity = QuantityOnHand;
        QuantityOnHand = newQuantity;

        AddMovementEvent(Math.Abs(quantityDelta), MovementType.Adjusted, snapshotQuantity, null, reason);
        AddEvent(new InventoryAdjustedEvent(Id, WarehouseId, ProductId, VariantId, quantityDelta, QuantityOnHand, reason));

        if (quantityDelta < 0)
            CheckLowStock();
        
        IncreaseVersion();
    }

    public void UpdateLowStockThreshold(int threshold)
    {
        if (threshold < 0)
            throw new DomainException("Low stock threshold cannot be negative");

        LowStockThreshold = threshold;
        IncreaseVersion();
    }

    private void AddMovementEvent(int quantity, MovementType type, int snapshotQuantity, Guid? orderId = null, string? reference = null)
    {
        AddEvent(new InventoryMovementCreatedEvent(
            Id, WarehouseId, ProductId, VariantId, orderId, quantity, type, snapshotQuantity, reference));
    }

    private void CheckLowStock()
    {
        if (QuantityOnHand < 0)
            throw new DomainException("Quantity on hand cannot be negative");

        if (QuantityAvailable <= LowStockThreshold)
        {
            AddEvent(new LowStockWarningEvent(Id, WarehouseId, ProductId, VariantId, Sku.Value, QuantityAvailable, LowStockThreshold));
        }
    }
}
