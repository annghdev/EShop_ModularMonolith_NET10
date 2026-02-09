using Contracts.Responses.Inventory;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Inventory.Application;

public class GetWarehouses
{
    public record Query() : IRequest<List<WarehouseDto>>;

    public class Handler(IInventoryUnitOfWork uow) : IRequestHandler<Query, List<WarehouseDto>>
    {
        public async Task<List<WarehouseDto>> Handle(Query query, CancellationToken cancellationToken)
        {
            var warehouses = await uow.Warehouses
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return warehouses.Select(w => new WarehouseDto(
                w.Id,
                w.Code,
                w.Name,
                w.Address?.Street,
                w.Address?.City,
                w.Address?.District,
                w.Address?.Country,
                w.Address?.PostalCode,
                w.IsActive,
                w.IsDefault)).ToList();
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("api/admin/inventory/warehouses", async (ISender sender) =>
                Results.Ok(await sender.Send(new Query())))
            .WithTags("Admin Inventory")
            .WithName("GetWarehouses")
            .RequireAuthorization();
        }
    }
}
