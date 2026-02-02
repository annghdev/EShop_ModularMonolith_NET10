using Catalog.Domain;
using Contracts.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Application;

/// <summary>
/// Get all product drafts for admin management.
/// </summary>
public class GetProductDrafts
{
    public record Query() : IRequest<List<ProductSearchDto>>;

    public class Handler(ICatalogUnitOfWork uow) : IRequestHandler<Query, List<ProductSearchDto>>
    {
        public async Task<List<ProductSearchDto>> Handle(Query query, CancellationToken cancellationToken)
        {
            var drafts = await uow.Products.AsQueryable()
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Variants)
                .Where(p => p.Status == ProductStatus.Draft)
                .OrderByDescending(p => p.CreatedAt)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return drafts.Select(p => new ProductSearchDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Slug = p.Slug.Value,
                Sku = p.Variants.FirstOrDefault()?.Sku.Value ?? string.Empty,
                Price = new MoneyDto(p.Price.Amount, p.Price.Currency),
                CategoryName = p.Category?.Name ?? string.Empty,
                BrandName = p.Brand?.Name ?? string.Empty,
                Thumbnail = p.Thumbnail?.Path,
                Status = p.Status.ToString(),
                VariantCount = p.Variants.Count,
                CreatedAt = p.CreatedAt.DateTime
            }).ToList();
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("api/admin/products/drafts", async (ISender sender) =>
            {
                var result = await sender.Send(new Query());
                return Results.Ok(result);
            })
            .WithTags("Admin Products")
            .WithName("GetProductDrafts")
            .RequireAuthorization();
        }
    }
}
