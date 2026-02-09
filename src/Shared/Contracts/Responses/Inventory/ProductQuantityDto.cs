namespace Contracts.Responses.Inventory;

/// <summary>
/// Represents a product with its variant quantities across warehouses
/// </summary>
public record ProductQuantityDto(
    Guid ProductId,
    string ProductName,
    string? Thumbnail,
    List<VariantQuantityDto> Variants);
