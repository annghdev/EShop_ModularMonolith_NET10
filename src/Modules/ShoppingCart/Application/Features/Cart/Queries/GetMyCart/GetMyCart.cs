using Contracts.Responses.ShoppingCart;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ShoppingCart.Application;

public class GetMyCart
{
    public record Query(Guid? CustomerId, string? GuestId) : IQuery<CartDto?>
    {
        public string CacheKey => $"shoppingcart:get:{CustomerId?.ToString() ?? GuestId}";
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

    public class Handler(IShoppingCartUnitOfWork uow) : IRequestHandler<Query, CartDto?>
    {
        public async Task<CartDto?> Handle(Query query, CancellationToken cancellationToken)
        {
            var cart = await uow.Carts.GetActiveCartAsync(query.CustomerId, query.GuestId, cancellationToken);
            if (cart is null)
            {
                return null;
            }

            return CartFeatureSupport.ToCartDto(cart);
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("api/cart", async (
                ISender sender,
                IUserContext userContext,
                IHttpContextAccessor httpContextAccessor) =>
            {
                var owner = CartFeatureSupport.ResolveOwner(userContext, httpContextAccessor);
                var result = await sender.Send(new Query(owner.CustomerId, owner.GuestId));
                return Results.Ok(result);
            })
            .WithTags("Shopping Cart")
            .WithName("GetMyCart");
        }
    }
}
