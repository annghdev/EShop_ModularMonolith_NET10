namespace Inventory.Domain;

public class StockItem : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public Sku Sku { get; private set; }
    public int Quantity { get; private set; }
    public int ThresholdWarning { get; private set; }


    private readonly List<StockReservation> _reservations = [];
    public IReadOnlyCollection<StockReservation> Reservations => _reservations.AsReadOnly();

    public int ReservedQuantity => _reservations.Sum(r => r.Quantity);
    public int AvailableQuantity => Quantity - ReservedQuantity;


    private StockItem() { } // EF Core

    public static StockItem Create(string name, Sku sku, int quantity, int thesholdWarning = 5)
        => new StockItem()
        {
            Name = name,
            Sku = sku,
            Quantity = quantity,
            ThresholdWarning = thesholdWarning
        };

    public void Export(int exportQuantity)
    {
        var snapshotQuantity = Quantity;
        Quantity -= exportQuantity;

        AddStockLog(null, exportQuantity, StockTransactionType.Export, snapshotQuantity);
        CheckLowQuantity();

        AddEvent(new StockItemExportedEvent(Id, Name, Sku.Value, exportQuantity, Quantity));

        IncreaseVersion();
    }

    public void Import(int importQuantity)
    {
        var snapshotQuantity = Quantity;
        Quantity += importQuantity;

        AddStockLog(null, importQuantity, StockTransactionType.Import, snapshotQuantity);
        AddEvent(new StockItemImportedEvent(Id, Name, Sku.Value, importQuantity, Quantity));

        IncreaseVersion();
    }
    public void ReserveStock(Guid orderId, int quantity)
    {
        if (orderId == Guid.Empty)
            throw new DomainException("Order ID cannot be empty");

        if (quantity <= 0)
            throw new DomainException("Reserve quantity must be greater than 0");

        if (AvailableQuantity < quantity)
            throw new DomainException($"Insufficient stock. Available: {AvailableQuantity}, Requested: {quantity}");

        var snapshotQuantity = Quantity;

        // Check if already reserved for this order
        var existingReservation = _reservations.FirstOrDefault(r => r.OrderId == orderId);
        if (existingReservation != null)
        {
            // Update existing reservation
            existingReservation.UpdateQuantity(quantity);
        }
        else
        {
            // Create new reservation
            var reservation = new StockReservation(orderId, quantity, Id);
            _reservations.Add(reservation);
        }

        AddStockLog(orderId, quantity, StockTransactionType.Reserve, snapshotQuantity);
        AddEvent(new StockReservedEvent(Id, orderId, quantity, AvailableQuantity));
        CheckLowQuantity();
        IncreaseVersion();
    }

    public void ReleaseStock(Guid orderId)
    {
        if (orderId == Guid.Empty)
            throw new DomainException("Order ID cannot be empty");

        var reservation = _reservations.FirstOrDefault(r => r.OrderId == orderId);
        if (reservation == null)
            return; // No reservation to release

        var snapshotQuantity = Quantity;
        var releasedQuantity = reservation.Quantity;
        _reservations.Remove(reservation);

        AddStockLog(orderId, releasedQuantity, StockTransactionType.Release, snapshotQuantity);
        AddEvent(new StockReleasedEvent(Id, orderId, releasedQuantity, AvailableQuantity));
        IncreaseVersion();
    }

    public void ConfirmStock(Guid orderId)
    {
        if (orderId == Guid.Empty)
            throw new DomainException("Order ID cannot be empty");

        var reservation = _reservations.FirstOrDefault(r => r.OrderId == orderId);
        if (reservation == null)
            throw new DomainException($"No reservation found for order {orderId}");

        var snapshotQuantity = Quantity;
        // Actually reduce the stock
        Quantity -= reservation.Quantity;
        var confirmedQuantity = reservation.Quantity;
        _reservations.Remove(reservation);

        AddStockLog(orderId, confirmedQuantity, StockTransactionType.Confirm, snapshotQuantity);
        AddEvent(new StockConfirmedEvent(Id, orderId, confirmedQuantity, Quantity));
        CheckLowQuantity();
        IncreaseVersion();
    }

    private void AddStockLog(Guid? orderId, int quantity, StockTransactionType type, int? snapshotQuantity = null)
    {
        AddEvent(new StockLogCreatedEvent(Id, orderId, quantity, snapshotQuantity ?? Quantity, type));
    }

    private void CheckLowQuantity()
    {
        if (Quantity < 0)
        {
            throw new DomainException("Quantity must be equal or greater than 0");
        }

        if (AvailableQuantity <= ThresholdWarning)
        {
            AddEvent(new LowStockThresholdWarningReachedEvent(Id, Name, Sku.Value, AvailableQuantity));
        }
    }
}
