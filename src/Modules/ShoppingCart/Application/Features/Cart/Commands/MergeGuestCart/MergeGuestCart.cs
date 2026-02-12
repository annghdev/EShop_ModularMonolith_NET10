using Contracts.Requests.ShoppingCart;
using Contracts.Responses.ShoppingCart;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ShoppingCart.Domain;

namespace ShoppingCart.Application;

public class MergeGuestCart
{
    public record Command(MergeGuestCartRequest Request, Guid CustomerId) : ICommand<CartDto>
    {
        public IEnumerable<string> CacheKeysToInvalidate => [];
        public IEnumerable<string> CacheKeyPrefix => ["shoppingcart:get:", "shoppingcart:summary:"];
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.CustomerId)
                .NotEmpty().WithMessage("Authenticated user is required.");

            RuleFor(c => c.Request.GuestId)
                .NotEmpty().WithMessage("GuestId is required.")
                .MaximumLength(100).WithMessage("GuestId cannot be greater than 100 characters.");
        }
    }

    public class Handler(IShoppingCartUnitOfWork uow) : IRequestHandler<Command, CartDto>
    {
        public async Task<CartDto> Handle(Command command, CancellationToken cancellationToken)
        {
            var guestId = command.Request.GuestId.Trim();
            var guestActiveCart = await uow.Carts.GetActiveCartAsync(null, guestId, cancellationToken);
            var customerActiveCart = await uow.Carts.GetActiveCartAsync(command.CustomerId, null, cancellationToken);

            Cart customerCart;
            if (customerActiveCart is null)
            {
                customerCart = Cart.CreateForCustomer(command.CustomerId);
                uow.Carts.Add(customerCart);
            }
            else
            {
                customerCart = await uow.Carts.LoadFullAggregate(customerActiveCart.Id, changeTracking: true);
            }

            if (guestActiveCart is null)
            {
                if (customerActiveCart is null)
                {
                    await uow.CommitAsync(cancellationToken);
                }

                return CartFeatureSupport.ToCartDto(customerCart);
            }

            if (guestActiveCart.Id == customerCart.Id)
            {
                return CartFeatureSupport.ToCartDto(customerCart);
            }

            var guestCart = await uow.Carts.LoadFullAggregate(guestActiveCart.Id, changeTracking: true);
            if (guestCart.Status == CartStatus.Active)
            {
                customerCart.MergeFrom(guestCart);
                guestCart.MarkAsMerged(customerCart.Id);
            }

            await uow.CommitAsync(cancellationToken);
            return CartFeatureSupport.ToCartDto(customerCart);
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("api/cart/merge-guest", async (
                MergeGuestCartRequest request,
                ISender sender,
                IUserContext userContext) =>
            {
                var result = await sender.Send(new Command(request, userContext.UserId));
                return Results.Ok(result);
            })
            .WithTags("Shopping Cart")
            .WithName("MergeGuestCart")
            .RequireAuthorization();
        }
    }
}
