namespace Contracts.Responses.ShoppingCart;

public record CartDto(
    Guid CartId,
    string OwnerType,
    Guid? CustomerId,
    string? GuestId,
    string Status,
    string? AppliedCouponCode,
    DateTimeOffset? LastActivityAt,
    IReadOnlyList<CartItemDto> Items,
    CartSummaryDto Summary);
