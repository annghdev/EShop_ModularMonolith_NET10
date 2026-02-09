namespace Contracts.Requests.Inventory;

/// <summary>
/// Request for importing inventory item by SKU
/// </summary>
public record ImportItemBySkuRequest(
    Guid WarehouseId,
    string Sku,
    int Quantity);
