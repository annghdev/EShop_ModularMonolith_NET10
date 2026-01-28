using Contracts.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Application;

/// <summary>
/// Get all brands for dropdown/filter in admin UI.
/// </summary>
public class GetBrands
{
    public record Query() : IQuery<List<BrandDto>>
    {
        public string CacheKey => "brands_all";
        public TimeSpan? ExpirationSliding => TimeSpan.FromHours(1);
    }

    public class Handler(ICatalogUnitOfWork uow) : IRequestHandler<Query, List<BrandDto>>
    {
        public async Task<List<BrandDto>> Handle(Query query, CancellationToken cancellationToken)
        {
            var brands = await uow.Brands
                .OrderBy(b => b.Name)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return brands.Select(b => new BrandDto
            {
                Id = b.Id,
                Name = b.Name,
                Logo = b.Logo
            }).ToList();
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("api/brands", async (ISender sender) =>
            {
                var result = await sender.Send(new Query());
                return Results.Ok(result);
            })
            .WithTags("Brands")
            .WithName("GetBrands");
        }
    }
}
