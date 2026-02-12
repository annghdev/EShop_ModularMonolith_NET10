using Contracts;
using Contracts.Responses.ShoppingCart;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ShoppingCart.Application;

public class GetCartItemCount
{
    public record Query(Guid? CustomerId, string? GuestId) : IQuery<CartSummaryDto>
    {
        public string CacheKey => $"shoppingcart:summary:{CustomerId?.ToString() ?? GuestId}";
        public TimeSpan? ExpirationSliding => TimeSpan.FromMinutes(1);
    }

    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(q => q)
                .Must(q => q.CustomerId.HasValue || !string.IsNullOrWhiteSpace(q.GuestId))
                .WithMessage("CustomerId or GuestId is required.");
        }
    }

    public class Handler(IShoppingCartUnitOfWork uow) : IRequestHandler<Query, CartSummaryDto>
    {
        public async Task<CartSummaryDto> Handle(Query query, CancellationToken cancellationToken)
        {
            var cart = await uow.Carts.GetActiveCartAsync(query.CustomerId, query.GuestId, cancellationToken);
            if (cart is null)
            {
                return new CartSummaryDto(
                    ItemCount: 0,
                    TotalQuantity: 0,
                    SubTotal: new MoneyDto(0, "VND"),
                    TotalDiscount: new MoneyDto(0, "VND"),
                    EstimatedTotal: new MoneyDto(0, "VND"));
            }

            var cartDto = CartFeatureSupport.ToCartDto(cart);
            return cartDto.Summary;
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("api/cart/count", async (
                ISender sender,
                IUserContext userContext,
                IHttpContextAccessor httpContextAccessor) =>
            {
                var owner = CartFeatureSupport.ResolveOwner(userContext, httpContextAccessor);
                var result = await sender.Send(new Query(owner.CustomerId, owner.GuestId));
                return Results.Ok(result);
            })
            .WithTags("Shopping Cart")
            .WithName("GetCartItemCount");
        }
    }
}
