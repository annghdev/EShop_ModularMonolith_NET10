namespace Contracts.Requests.Inventory;

/// <summary>
/// Request for getting paginated list of stocking products
/// </summary>
public record GetStockingProductsRequest(
    int Page = 1,
    int PageSize = 20,
    string? Filter = null,
    Guid? WarehouseId = null,
    bool InStockOnly = false);
