using Contracts.Responses.ShoppingCart;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ShoppingCart.Application;

public class ClearCart
{
    public record Command(Guid? CustomerId, string? GuestId) : ICommand<CartDto?>
    {
        public IEnumerable<string> CacheKeysToInvalidate => [];
        public IEnumerable<string> CacheKeyPrefix => ["shoppingcart:get:", "shoppingcart:summary:"];
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c)
                .Must(c => c.CustomerId.HasValue || !string.IsNullOrWhiteSpace(c.GuestId))
                .WithMessage("CustomerId or GuestId is required.");
        }
    }

    public class Handler(IShoppingCartUnitOfWork uow) : IRequestHandler<Command, CartDto?>
    {
        public async Task<CartDto?> Handle(Command command, CancellationToken cancellationToken)
        {
            var activeCart = await uow.Carts.GetActiveCartAsync(command.CustomerId, command.GuestId, cancellationToken);
            if (activeCart is null)
            {
                return null;
            }

            var cart = await uow.Carts.LoadFullAggregate(activeCart.Id, changeTracking: true);
            cart.Clear();

            await uow.CommitAsync(cancellationToken);
            return CartFeatureSupport.ToCartDto(cart);
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapDelete("api/cart/items", async (
                ISender sender,
                IUserContext userContext,
                IHttpContextAccessor httpContextAccessor) =>
            {
                var owner = CartFeatureSupport.ResolveOwner(userContext, httpContextAccessor);
                var result = await sender.Send(new Command(owner.CustomerId, owner.GuestId));
                return Results.Ok(result);
            })
            .WithTags("Shopping Cart")
            .WithName("ClearCart");
        }
    }
}
