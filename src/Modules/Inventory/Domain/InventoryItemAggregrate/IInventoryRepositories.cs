namespace Inventory.Domain;

public interface IInventoryItemRepository : IRepository<InventoryItem>
{
    /// <summary>
    /// Get inventory item by variant ID, optionally filtered by warehouse
    /// </summary>
    Task<InventoryItem?> GetByVariantIdAsync(Guid variantId, Guid? warehouseId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all inventory items for a product across all warehouses
    /// </summary>
    Task<IEnumerable<InventoryItem>> GetAllByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all inventory items for a variant across all warehouses
    /// </summary>
    Task<IEnumerable<InventoryItem>> GetAllByVariantIdAsync(Guid variantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get inventory item by SKU, optionally filtered by warehouse
    /// </summary>
    Task<InventoryItem?> GetBySkuAsync(string sku, Guid? warehouseId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get inventory items with low stock below threshold
    /// </summary>
    Task<IEnumerable<InventoryItem>> GetLowStockItemsAsync(Guid? warehouseId = null, CancellationToken cancellationToken = default);
}
