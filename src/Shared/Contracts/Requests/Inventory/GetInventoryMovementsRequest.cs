namespace Contracts.Requests.Inventory;

/// <summary>
/// Request for inventory movement history
/// </summary>
public record GetInventoryMovementsRequest(
    int Page = 1,
    int PageSize = 20,
    string? Keyword = null,
    Guid? WarehouseId = null,
    string? Type = null);
