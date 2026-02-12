namespace Contracts.Responses.ShoppingCart;

public record CartSummaryDto(
    int ItemCount,
    int TotalQuantity,
    MoneyDto SubTotal,
    MoneyDto TotalDiscount,
    MoneyDto EstimatedTotal);
