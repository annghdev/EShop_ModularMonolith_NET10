namespace Contracts.Responses.Inventory;

/// <summary>
/// Represents a stocking product in inventory list
/// </summary>
public record StockingProductDto(
    Guid ProductId,
    string ProductName,
    string? Sku,
    string? Thumbnail,
    int VariantCount,
    int TotalQuantity,
    List<VariantStockSummaryDto> Variants);

/// <summary>
/// Summary of variant stock for list display
/// </summary>
public record VariantStockSummaryDto(
    Guid VariantId,
    string Sku,
    string? AttributeValues,
    int TotalQuantity);
