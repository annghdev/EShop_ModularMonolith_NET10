namespace Contracts.Requests.Inventory;

/// <summary>
/// Request to adjust the quantity of a single variant in a warehouse
/// </summary>
public record AdjustVariantQuantityRequest(
    Guid WarehouseId,
    Guid ProductId,
    Guid VariantId,
    int NewQuantity,
    string? Reason = null);
