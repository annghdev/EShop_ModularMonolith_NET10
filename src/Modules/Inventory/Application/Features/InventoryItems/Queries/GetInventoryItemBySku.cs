using Contracts.Responses.Inventory;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Inventory.Application;

public class GetInventoryItemBySku
{
    public record Query(string Sku, Guid WarehouseId) : IRequest<InventoryItemDto>;

    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(q => q.Sku)
                .NotEmpty().WithMessage("Sku is required")
                .MaximumLength(50).WithMessage("Sku cannot be greater than 50 characters");

            RuleFor(q => q.WarehouseId)
                .NotEmpty().WithMessage("WarehouseId is required");
        }
    }

    public class Handler(IInventoryUnitOfWork uow) : IRequestHandler<Query, InventoryItemDto>
    {
        public async Task<InventoryItemDto> Handle(Query query, CancellationToken cancellationToken)
        {
            var item = await uow.InventoryItemRepository
                .GetBySkuAndWarehouseAsync(query.Sku, query.WarehouseId, cancellationToken);

            if (item is null)
            {
                throw new NotFoundException("Inventory item", query.Sku);
            }

            return new InventoryItemDto(
                item.Id,
                item.WarehouseId,
                item.Warehouse?.Code ?? string.Empty,
                item.ProductId,
                item.ProductName,
                item.VariantId,
                item.Sku.Value,
                item.QuantityOnHand,
                item.QuantityReserved,
                item.QuantityAvailable,
                item.LowStockThreshold);
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("api/admin/inventory/items/by-sku", async (
                string sku,
                Guid warehouseId,
                ISender sender) =>
            {
                var result = await sender.Send(new Query(sku, warehouseId));
                return Results.Ok(result);
            })
            .WithTags("Admin Inventory")
            .WithName("GetInventoryItemBySku")
            .RequireAuthorization();
        }
    }
}
