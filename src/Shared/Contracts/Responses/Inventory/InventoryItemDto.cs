namespace Contracts.Responses.Inventory;

/// <summary>
/// Response DTO for inventory item detail
/// </summary>
public record InventoryItemDto(
    Guid Id,
    Guid WarehouseId,
    string WarehouseCode,
    Guid ProductId,
    string ProductName,
    Guid VariantId,
    string Sku,
    int QuantityOnHand,
    int QuantityReserved,
    int QuantityAvailable,
    int LowStockThreshold);
