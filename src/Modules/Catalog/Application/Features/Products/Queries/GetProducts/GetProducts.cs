using Catalog.Domain;
using Contracts.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Application;

public class GetProducts
{
    private static readonly Random _random = new();
    private static readonly string[] FeaturedTags = ["Hot", "New", "None", "None", "None"]; // 40% chance for Hot/New

    public record Query(
        int Page = 1,
        int PageSize = 20) : IRequest<PaginatedResult<ProductCardDto>>;

    public class Handler(ICatalogUnitOfWork uow) : IRequestHandler<Query, PaginatedResult<ProductCardDto>>
    {
        public async Task<PaginatedResult<ProductCardDto>> Handle(Query query, CancellationToken cancellationToken)
        {
            var dbQuery = uow.Products.AsQueryable()
                .Where(p => p.Status == ProductStatus.Published)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Variants)
                .Include(p => p.Attributes)
                    .ThenInclude(pa => pa.Attribute)
                        .ThenInclude(a => a!.Values)
                .AsNoTracking();

            // Get total count before pagination
            var total = await dbQuery.CountAsync(cancellationToken);

            // Pagination
            var items = await dbQuery
                .OrderByDescending(p => p.CreatedAt)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync(cancellationToken);

            // Map to ProductCardDto
            var dtos = items.Select(MapToProductCardDto).ToList();

            return new PaginatedResult<ProductCardDto>(query.Page, query.PageSize, dtos, total);
        }

        private static ProductCardDto MapToProductCardDto(Product p)
        {
            // Generate hardcoded random values for demo
            var discountPercent = _random.Next(10, 71); // 10-70%
            var originalPrice = p.Price.Amount;
            var discountedPrice = Math.Round(originalPrice * (100 - discountPercent) / 100, 0);
            var rating = Math.Round(_random.NextDouble() * 1.0 + 4.0, 1); // 4.0-5.0
            var soldCount = _random.Next(50, 5001);
            var feedbackCount = _random.Next(10, 501);
            var featuredTag = FeaturedTags[_random.Next(FeaturedTags.Length)];

            // Get secondary image from product images
            var secondaryImage = p.Images.Count > 0
                ? p.Images[_random.Next(p.Images.Count)].Path
                : null;

            // Map variant dots from ProductAttributes with HasVariant = true
            var variantDots = p.Attributes
                .Where(pa => pa.HasVariant && pa.Attribute != null)
                .OrderBy(pa => pa.DisplayOrder)
                .Select(pa => new VariantDotDto
                {
                    AttributeName = pa.Attribute!.Name,
                    DisplayType = pa.Attribute.DisplayText ? "text" : "color",
                    ValueStyleCss = pa.Attribute.ValueStyleCss,
                    Values = pa.Attribute.Values.Select(v => new VariantDotValueDto
                    {
                        Id = v.Id,
                        Value = v.Name,
                        ColorCode = v.ColorCode
                    }).ToList()
                })
                .ToList();

            return new ProductCardDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Slug = p.Slug.Value,
                OriginalPrice = originalPrice,
                DiscountPercent = discountPercent,
                DiscountedPrice = discountedPrice,
                Currency = p.Price.Currency,
                Rating = (decimal)rating,
                SoldCount = soldCount,
                FeedbackCount = feedbackCount,
                FeaturedTag = featuredTag,
                Thumbnail = p.Thumbnail?.Path,
                SecondaryImage = secondaryImage,
                VariantDots = variantDots
            };
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("api/products", async (
                int page = 1,
                int pageSize = 20,
                ISender sender = null!) =>
            {
                var query = new Query(page, pageSize);
                var result = await sender.Send(query);
                return Results.Ok(result);
            })
            .WithTags("Products")
            .WithName("GetProducts");
        }
    }
}
