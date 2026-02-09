using Contracts;
using Contracts.Requests.Inventory;
using Contracts.Responses.Inventory;
using FluentValidation;
using Inventory.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Inventory.Application;

public class GetInventoryMovements
{
    public record Query(GetInventoryMovementsRequest Request) : IRequest<PaginatedResult<InventoryMovementDto>>;

    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(q => q.Request.Page)
                .GreaterThan(0).WithMessage("Page must be greater than 0");

            RuleFor(q => q.Request.PageSize)
                .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100");

            RuleFor(q => q.Request.Keyword)
                .MaximumLength(200).WithMessage("Keyword cannot be greater than 200 characters");
        }
    }

    public class Handler(IInventoryUnitOfWork uow) : IRequestHandler<Query, PaginatedResult<InventoryMovementDto>>
    {
        public async Task<PaginatedResult<InventoryMovementDto>> Handle(Query query, CancellationToken cancellationToken)
        {
            var request = query.Request;

            var movements = uow.InventoryMovements.AsNoTracking();
            var items = uow.InventoryItems.AsNoTracking();
            var warehouses = uow.Warehouses.AsNoTracking();

            var baseQuery =
                from movement in movements
                join item in items on movement.InventoryItemId equals item.Id
                join warehouse in warehouses on movement.WarehouseId equals warehouse.Id
                select new { movement, item, warehouse };

            if (request.WarehouseId.HasValue)
            {
                baseQuery = baseQuery.Where(x => x.movement.WarehouseId == request.WarehouseId.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var keyword = request.Keyword.Trim().ToLower();
                baseQuery = baseQuery.Where(x =>
                    x.item.ProductName.ToLower().Contains(keyword) ||
                    x.item.Sku.Value.ToLower().Contains(keyword));
            }

            if (!string.IsNullOrWhiteSpace(request.Type) &&
                Enum.TryParse<MovementType>(request.Type, true, out var type))
            {
                baseQuery = baseQuery.Where(x => x.movement.Type == type);
            }

            var total = await baseQuery.CountAsync(cancellationToken);

            var paged = await baseQuery
                .OrderByDescending(x => x.movement.CreatedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new InventoryMovementDto(
                    x.movement.Id,
                    x.movement.CreatedAt,
                    x.movement.WarehouseId,
                    x.warehouse.Name,
                    x.movement.ProductId,
                    x.item.ProductName,
                    x.movement.VariantId,
                    x.item.Sku.Value,
                    x.movement.Type.ToString(),
                    x.movement.Quantity,
                    x.movement.SnapshotQuantity,
                    x.movement.OrderId,
                    x.movement.Reference,
                    x.movement.Notes))
                .ToListAsync(cancellationToken);

            return new PaginatedResult<InventoryMovementDto>(request.Page, request.PageSize, paged, total);
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("api/admin/inventory/movements", async (
                string? keyword,
                Guid? warehouseId,
                string? type,
                int page = 1,
                int pageSize = 20,
                ISender sender = null!) =>
            {
                var request = new GetInventoryMovementsRequest(page, pageSize, keyword, warehouseId, type);
                var result = await sender.Send(new Query(request));
                return Results.Ok(result);
            })
            .WithTags("Admin Inventory")
            .WithName("GetInventoryMovements")
            .RequireAuthorization();
        }
    }
}
