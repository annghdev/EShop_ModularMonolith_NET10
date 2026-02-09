using Contracts.Requests.Inventory;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Inventory.Application;

public class AdjustInventoryItemQuantity
{
    public record Command(AdjustVariantQuantityRequest Request) : ICommand
    {
        public IEnumerable<string> CacheKeysToInvalidate => [];
        public IEnumerable<string> CacheKeyPrefix => [];
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.Request.WarehouseId)
                .NotEmpty().WithMessage("WarehouseId is required");

            RuleFor(c => c.Request.ProductId)
                .NotEmpty().WithMessage("ProductId is required");

            RuleFor(c => c.Request.VariantId)
                .NotEmpty().WithMessage("VariantId is required");

            RuleFor(c => c.Request.NewQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("NewQuantity must be greater than or equal to 0");

            RuleFor(c => c.Request.Reason)
                .MaximumLength(200).WithMessage("Reason cannot be greater than 200 characters");
        }
    }

    public class Handler(IInventoryUnitOfWork uow) : IRequestHandler<Command>
    {
        public async Task Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            var item = await uow.InventoryItemRepository
                .GetByVariantIdAsync(request.VariantId, request.WarehouseId, cancellationToken)
                ?? throw new NotFoundException("Inventory item", request.VariantId);

            if (item.ProductId != request.ProductId)
            {
                throw new DomainException("ProductId does not match inventory item");
            }

            var delta = request.NewQuantity - item.QuantityOnHand;
            if (delta == 0)
            {
                return;
            }

            var reason = string.IsNullOrWhiteSpace(request.Reason) ? "Manual adjustment" : request.Reason!;
            item.Adjust(delta, reason);

            await uow.CommitAsync(cancellationToken);
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPut("api/admin/inventory/items/adjust", async (
                AdjustVariantQuantityRequest request,
                ISender sender) =>
            {
                await sender.Send(new Command(request));
                return Results.Accepted();
            })
            .WithTags("Admin Inventory")
            .WithName("AdjustInventoryItemQuantity")
            .RequireAuthorization();
        }
    }
}
