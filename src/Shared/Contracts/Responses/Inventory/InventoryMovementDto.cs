namespace Contracts.Responses.Inventory;

/// <summary>
/// Inventory movement history item
/// </summary>
public record InventoryMovementDto(
    Guid Id,
    DateTimeOffset CreatedAt,
    Guid WarehouseId,
    string WarehouseName,
    Guid ProductId,
    string ProductName,
    Guid VariantId,
    string Sku,
    string Type,
    int Quantity,
    int SnapshotQuantity,
    Guid? OrderId,
    string? Reference,
    string? Notes);
