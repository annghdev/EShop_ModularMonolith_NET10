namespace Contracts.Responses.ShoppingCart;

public record CartItemDto(
    Guid ItemId,
    Guid ProductId,
    Guid VariantId,
    string Sku,
    string ProductName,
    string VariantName,
    string? Thumbnail,
    int Quantity,
    MoneyDto OriginalPrice,
    MoneyDto UnitPrice,
    MoneyDto DiscountAmount,
    MoneyDto LineTotal,
    MoneyDto LineTotalDiscount);
