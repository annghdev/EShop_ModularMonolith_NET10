using Contracts.Responses.Inventory;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Inventory.Application;

public class GetProductInventory
{
    public record Query(Guid ProductId) : IRequest<ProductQuantityDto>;

    public class Handler(IInventoryUnitOfWork uow) : IRequestHandler<Query, ProductQuantityDto>
    {
        public async Task<ProductQuantityDto> Handle(Query query, CancellationToken cancellationToken)
        {
            var items = await uow.InventoryItems
                .Include(i => i.Warehouse)
                .AsNoTracking()
                .Where(i => i.ProductId == query.ProductId)
                .ToListAsync(cancellationToken);

            if (items.Count == 0)
            {
                throw new NotFoundException("Inventory items", query.ProductId);
            }

            var variants = items.Select(i => new VariantQuantityDto(
                i.VariantId,
                i.Sku.Value,
                i.WarehouseId,
                i.Warehouse?.Name ?? string.Empty,
                i.QuantityOnHand,
                i.QuantityReserved,
                i.QuantityAvailable)).ToList();

            return new ProductQuantityDto(
                query.ProductId,
                items.First().ProductName,
                null,
                variants);
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("api/admin/inventory/products/{productId:guid}", async (
                Guid productId,
                ISender sender) =>
            {
                var result = await sender.Send(new Query(productId));
                return Results.Ok(result);
            })
            .WithTags("Admin Inventory")
            .WithName("GetProductInventory")
            .RequireAuthorization();
        }
    }
}
