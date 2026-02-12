using Contracts;
using Contracts.Requests.ShoppingCart;
using Contracts.Responses.ShoppingCart;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ShoppingCart.Domain;

namespace ShoppingCart.Application;

public class AddCartItem
{
    public record Command(
        AddCartItemRequest Request,
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
            RuleFor(c => c.Request.Sku)
                .NotEmpty().WithMessage("Sku is required.")
                .MaximumLength(50).WithMessage("Sku cannot be greater than 50 characters.");

            RuleFor(c => c.Request.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0.");

            RuleFor(c => c)
                .Must(c => c.CustomerId.HasValue || !string.IsNullOrWhiteSpace(c.GuestId))
                .WithMessage("CustomerId or GuestId is required.");
        }
    }

    public class Handler(IShoppingCartUnitOfWork uow, IIntegrationRequestSender requestSender)
        : IRequestHandler<Command, CartDto>
    {
        public async Task<CartDto> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;
            var productInfo = await requestSender.SendQueryAsync<GetProductVariantBySkuIntegrationQuery, ProductVariantBySkuResponse>(
                new GetProductVariantBySkuIntegrationQuery(ModuleNameConsts.ShoppingCart, request.Sku),
                cancellationToken);

            var unitPrice = new Money(productInfo.UnitPrice.Amount, productInfo.UnitPrice.Currency ?? "VND");

            var activeCart = await uow.Carts.GetActiveCartAsync(command.CustomerId, command.GuestId, cancellationToken);
            Cart cart;

            if (activeCart is null)
            {
                cart = command.CustomerId.HasValue
                    ? Cart.CreateForCustomer(command.CustomerId.Value)
                    : Cart.CreateForGuest(command.GuestId!);

                uow.Carts.Add(cart);
            }
            else
            {
                cart = await uow.Carts.LoadFullAggregate(activeCart.Id, changeTracking: true);
            }

            cart.AddItem(
                productInfo.ProductId,
                productInfo.VariantId,
                productInfo.Sku,
                productInfo.ProductName,
                productInfo.VariantName,
                productInfo.Thumbnail,
                unitPrice,
                request.Quantity);

            await uow.CommitAsync(cancellationToken);
            return CartFeatureSupport.ToCartDto(cart);
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("api/cart/items", async (
                AddCartItemRequest request,
                ISender sender,
                IUserContext userContext,
                IHttpContextAccessor httpContextAccessor) =>
            {
                var owner = CartFeatureSupport.ResolveOwner(userContext, httpContextAccessor);
                var result = await sender.Send(new Command(request, owner.CustomerId, owner.GuestId));
                return Results.Ok(result);
            })
            .WithTags("Shopping Cart")
            .WithName("AddCartItem");
        }
    }
}
