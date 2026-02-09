using Contracts.Responses.Inventory;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Inventory.Application;

public class GetActiveWarehouses
{
    public record Query() : IRequest<List<WarehouseSimpleDto>>;

    public class Handler(IInventoryUnitOfWork uow) : IRequestHandler<Query, List<WarehouseSimpleDto>>
    {
        public async Task<List<WarehouseSimpleDto>> Handle(Query query, CancellationToken cancellationToken)
        {
            var warehouses = await uow.WarehouseRepository.GetActiveWarehousesAsync(cancellationToken);

            return warehouses.Select(w => new WarehouseSimpleDto(
                w.Id,
                w.Code,
                w.Name,
                w.IsDefault)).ToList();
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("api/admin/inventory/warehouses/active", async (ISender sender) =>
                Results.Ok(await sender.Send(new Query())))
            .WithTags("Admin Inventory")
            .WithName("GetActiveWarehouses")
            .RequireAuthorization();
        }
    }
}
