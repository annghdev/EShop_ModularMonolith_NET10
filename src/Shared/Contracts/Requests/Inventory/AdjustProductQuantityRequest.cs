namespace Contracts.Requests.Inventory;

/// <summary>
/// Request to adjust quantities for multiple variants of a product in a warehouse
/// </summary>
public record AdjustProductQuantityRequest(
    Guid WarehouseId,
    Guid ProductId,
    List<VariantQuantityItem> Items,
    string? Reason = null);

/// <summary>
/// Item representing a variant and its new quantity
/// </summary>
public record VariantQuantityItem(
    Guid VariantId,
    int NewQuantity);
