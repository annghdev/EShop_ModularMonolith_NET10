namespace Contracts.Responses.Inventory;

/// <summary>
/// Represents quantity information for a single variant in a warehouse
/// </summary>
public record VariantQuantityDto(
    Guid VariantId,
    string Sku,
    Guid WarehouseId,
    string WarehouseName,
    int QuantityOnHand,
    int QuantityReserved,
    int QuantityAvailable);
