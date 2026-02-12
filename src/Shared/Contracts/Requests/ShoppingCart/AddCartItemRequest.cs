namespace Contracts.Requests.ShoppingCart;

public record AddCartItemRequest(
    string Sku,
    int Quantity = 1);
