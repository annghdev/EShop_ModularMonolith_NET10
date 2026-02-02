namespace Inventory.Domain;

/// <summary>
/// Aggregate root representing a physical warehouse/storage location
/// </summary>
public class Warehouse : AggregateRoot
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public Address? Address { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsDefault { get; private set; }

    private readonly List<InventoryItem> _inventoryItems = [];
    public IReadOnlyCollection<InventoryItem> InventoryItems => _inventoryItems.AsReadOnly();

    private Warehouse() { } // EF Core

    public static Warehouse Create(string code, string name, Address? address = null, bool isDefault = false)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("Warehouse code is required");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Warehouse name is required");

        var warehouse = new Warehouse
        {
            Code = code.ToUpperInvariant().Trim(),
            Name = name.Trim(),
            Address = address,
            IsActive = true,
            IsDefault = isDefault
        };

        warehouse.AddEvent(new WarehouseCreatedEvent(warehouse.Id, warehouse.Code, warehouse.Name, isDefault));

        return warehouse;
    }

    public void Update(string name, Address? address)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Warehouse name is required");

        Name = name.Trim();
        Address = address;

        IncreaseVersion();
    }

    public void Activate()
    {
        if (IsActive) return;

        IsActive = true;
        AddEvent(new WarehouseActivatedEvent(Id, Code));
        IncreaseVersion();
    }

    public void Deactivate()
    {
        if (!IsActive) return;

        if (_inventoryItems.Any(i => i.QuantityOnHand > 0 || i.QuantityReserved > 0))
            throw new DomainException("Cannot deactivate warehouse with existing inventory");

        IsActive = false;
        AddEvent(new WarehouseDeactivatedEvent(Id, Code));
        IncreaseVersion();
    }

    public void SetAsDefault()
    {
        if (IsDefault) return;

        if (!IsActive)
            throw new DomainException("Cannot set inactive warehouse as default");

        IsDefault = true;
        IncreaseVersion();
    }

    public void RemoveDefault()
    {
        if (!IsDefault) return;

        IsDefault = false;
        IncreaseVersion();
    }

    /// <summary>
    /// Get inventory item by variant ID
    /// </summary>
    public InventoryItem? GetInventoryItem(Guid variantId)
    {
        return _inventoryItems.FirstOrDefault(i => i.VariantId == variantId);
    }

    /// <summary>
    /// Get inventory item by SKU
    /// </summary>
    public InventoryItem? GetInventoryItemBySku(string sku)
    {
        return _inventoryItems.FirstOrDefault(i => i.Sku.Value == sku);
    }

    /// <summary>
    /// Get all inventory items for a product
    /// </summary>
    public IEnumerable<InventoryItem> GetInventoryItemsByProduct(Guid productId)
    {
        return _inventoryItems.Where(i => i.ProductId == productId);
    }
}
