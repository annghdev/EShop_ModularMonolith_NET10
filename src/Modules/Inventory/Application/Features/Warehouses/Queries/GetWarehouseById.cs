using Contracts.Responses.Inventory;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Inventory.Application;

public class GetWarehouseById
{
    public record Query(Guid WarehouseId) : IRequest<WarehouseDto>;

    public class Handler(IInventoryUnitOfWork uow) : IRequestHandler<Query, WarehouseDto>
    {
        public async Task<WarehouseDto> Handle(Query query, CancellationToken cancellationToken)
        {
            var warehouse = await uow.WarehouseRepository.GetByIdAsync(query.WarehouseId, cancellationToken)
                ?? throw new NotFoundException("Warehouse", query.WarehouseId);

            return new WarehouseDto(
                warehouse.Id,
                warehouse.Code,
                warehouse.Name,
                warehouse.Address?.Street,
                warehouse.Address?.City,
                warehouse.Address?.District,
                warehouse.Address?.Country,
                warehouse.Address?.PostalCode,
                warehouse.IsActive,
                warehouse.IsDefault);
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("api/admin/inventory/warehouses/{id:guid}", async (
                Guid id,
                ISender sender) =>
            {
                var result = await sender.Send(new Query(id));
                return Results.Ok(result);
            })
            .WithTags("Admin Inventory")
            .WithName("GetWarehouseById")
            .RequireAuthorization();
        }
    }
}
