using Contracts;
using Contracts.Requests.Inventory;
using Contracts.Responses.Inventory;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Inventory.Application;

public class GetStockingProducts
{
    public record Query(GetStockingProductsRequest Request) : IRequest<PaginatedResult<StockingProductDto>>;

    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(q => q.Request.Page)
                .GreaterThan(0).WithMessage("Page must be greater than 0");

            RuleFor(q => q.Request.PageSize)
                .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100");

            RuleFor(q => q.Request.Filter)
                .MaximumLength(200).WithMessage("Filter cannot be greater than 200 characters");
        }
    }

    public class Handler(IInventoryUnitOfWork uow) : IRequestHandler<Query, PaginatedResult<StockingProductDto>>
    {
        public async Task<PaginatedResult<StockingProductDto>> Handle(Query query, CancellationToken cancellationToken)
        {
            var request = query.Request;

            var dbQuery = uow.InventoryItems
                .AsNoTracking()
                .AsQueryable();

            if (request.WarehouseId.HasValue)
            {
                dbQuery = dbQuery.Where(i => i.WarehouseId == request.WarehouseId.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Filter))
            {
                var keyword = request.Filter.Trim().ToLower();
                dbQuery = dbQuery.Where(i =>
                    i.ProductName.ToLower().Contains(keyword) ||
                    i.Sku.Value.ToLower().Contains(keyword));
            }

            if (request.InStockOnly)
            {
                dbQuery = dbQuery.Where(i => i.QuantityOnHand > 0);
            }

            var items = await dbQuery.ToListAsync(cancellationToken);

            var grouped = items
                .GroupBy(i => i.ProductId)
                .Select(g =>
                {
                    var itemList = g.ToList();
                    var variants = itemList
                        .GroupBy(i => new { i.VariantId, Sku = i.Sku.Value })
                        .Select(v => new VariantStockSummaryDto(
                            v.Key.VariantId,
                            v.Key.Sku,
                            null,
                            v.Sum(x => x.QuantityOnHand)))
                        .ToList();

                    return new StockingProductDto(
                        g.Key,
                        itemList.First().ProductName,
                        itemList.First().Sku.Value,
                        null,
                        variants.Count,
                        itemList.Sum(x => x.QuantityOnHand),
                        variants);
                })
                .ToList();

            var total = grouped.Count;

            var paged = grouped
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return new PaginatedResult<StockingProductDto>(request.Page, request.PageSize, paged, total);
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("api/admin/inventory/products", async (
                string? filter,
                Guid? warehouseId,
                bool inStockOnly,
                int page = 1,
                int pageSize = 20,
                ISender sender = null!) =>
            {
                var request = new GetStockingProductsRequest(page, pageSize, filter, warehouseId, inStockOnly);
                var result = await sender.Send(new Query(request));
                return Results.Ok(result);
            })
            .WithTags("Admin Inventory")
            .WithName("GetStockingProducts")
            .RequireAuthorization();
        }
    }
}
