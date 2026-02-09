using Contracts;
using Contracts.Requests.Inventory;
using Contracts.Responses.Inventory;

namespace BlazorAdmin.Services;

public interface IInventoryService
{
    Task<PaginatedResult<StockingProductDto>> GetStockingProductsAsync(
        string? filter,
        Guid? warehouseId,
        bool inStockOnly,
        int page,
        int pageSize);

    Task<ProductQuantityDto?> GetProductInventoryAsync(Guid productId);

    Task<InventoryItemDto?> GetItemBySkuAsync(string sku, Guid warehouseId);

    Task AdjustItemQuantityAsync(AdjustVariantQuantityRequest request);

    Task ImportItemBySkuAsync(ImportItemBySkuRequest request);

    Task<PaginatedResult<InventoryMovementDto>> GetInventoryMovementsAsync(
        string? keyword,
        Guid? warehouseId,
        string? type,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
