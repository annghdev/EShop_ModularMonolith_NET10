using Contracts;
using Contracts.Responses.ShoppingCart;
using Microsoft.AspNetCore.Http;
using ShoppingCart.Domain;

namespace ShoppingCart.Application;

internal static class CartFeatureSupport
{
    internal const string GuestIdHeaderName = "X-Guest-Id";

    internal static (Guid? CustomerId, string? GuestId) ResolveOwner(
        IUserContext userContext,
        IHttpContextAccessor httpContextAccessor)
    {
        if (userContext.IsAuthenticated && userContext.UserId != Guid.Empty)
        {
            return (userContext.UserId, null);
        }

        var guestId = httpContextAccessor.HttpContext?.Request.Headers[GuestIdHeaderName].FirstOrDefault()?.Trim();
        return (null, string.IsNullOrWhiteSpace(guestId) ? null : guestId);
    }

    internal static CartDto ToCartDto(Cart cart)
    {
        var items = cart.Items
            .Select(item => new CartItemDto(
                item.Id,
                item.ProductId,
                item.VariantId,
                item.Sku,
                item.ProductName,
                item.VariantName,
                item.Thumbnail,
                item.Quantity,
                ToMoneyDto(item.OriginalPrice),
                ToMoneyDto(item.UnitPrice),
                ToMoneyDto(item.DiscountAmount),
                ToMoneyDto(item.LineTotal),
                ToMoneyDto(item.LineTotalDiscount)))
            .ToList();

        var summary = new CartSummaryDto(
            ItemCount: items.Count,
            TotalQuantity: items.Sum(i => i.Quantity),
            SubTotal: ToMoneyDto(cart.SubTotal),
            TotalDiscount: ToMoneyDto(cart.TotalDiscount),
            EstimatedTotal: ToMoneyDto(cart.EstimatedTotal));

        return new CartDto(
            CartId: cart.Id,
            OwnerType: cart.Customer.IsRegistered ? "Customer" : "Guest",
            CustomerId: cart.Customer.CustomerId,
            GuestId: cart.Customer.GuestId,
            Status: cart.Status.ToString(),
            AppliedCouponCode: cart.AppliedCouponCode,
            LastActivityAt: cart.LastActivityAt,
            Items: items,
            Summary: summary);
    }

    internal static MoneyDto ToMoneyDto(Money value)
    {
        return new MoneyDto(value.Amount, value.Currency);
    }
}
