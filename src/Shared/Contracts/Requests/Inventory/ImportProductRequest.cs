namespace Contracts.Requests.Inventory;

/// <summary>
/// Request to import a product into inventory with initial quantities
/// </summary>
public record ImportProductRequest(
    Guid WarehouseId,
    Guid ProductId,
    string ProductName,
    List<ImportVariantItem> Variants);

/// <summary>
/// Item representing a variant and its initial quantity for import
/// </summary>
public record ImportVariantItem(
    Guid VariantId,
    string Sku,
    int InitialQuantity);
