using Contracts.Responses.ShoppingCart;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ShoppingCart.Application;

public class RemoveCartItem
{
    public record Command(
        Guid VariantId,
        Guid? CustomerId,
        string? GuestId) : ICommand<CartDto>
    {
        public IEnumerable<string> CacheKeysToInvalidate => [];
        public IEnumerable<string> CacheKeyPrefix => ["shoppingcart:get:", "shoppingcart:summary:"];
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.VariantId)
                .NotEmpty().WithMessage("VariantId is required.");

            RuleFor(c => c)
                .Must(c => c.CustomerId.HasValue || !string.IsNullOrWhiteSpace(c.GuestId))
                .WithMessage("CustomerId or GuestId is required.");
        }
    }

    public class Handler(IShoppingCartUnitOfWork uow) : IRequestHandler<Command, CartDto>
    {
        public async Task<CartDto> Handle(Command command, CancellationToken cancellationToken)
        {
            var activeCart = await uow.Carts.GetActiveCartAsync(command.CustomerId, command.GuestId, cancellationToken)
                ?? throw new NotFoundException("Cart", $"{command.CustomerId?.ToString() ?? command.GuestId}");

            var cart = await uow.Carts.LoadFullAggregate(activeCart.Id, changeTracking: true);
            cart.RemoveItem(command.VariantId);

            await uow.CommitAsync(cancellationToken);
            return CartFeatureSupport.ToCartDto(cart);
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapDelete("api/cart/items/{variantId:guid}", async (
                Guid variantId,
                ISender sender,
                IUserContext userContext,
                IHttpContextAccessor httpContextAccessor) =>
            {
                var owner = CartFeatureSupport.ResolveOwner(userContext, httpContextAccessor);
                var result = await sender.Send(new Command(variantId, owner.CustomerId, owner.GuestId));
                return Results.Ok(result);
            })
            .WithTags("Shopping Cart")
            .WithName("RemoveCartItem");
        }
    }
}
