using Contracts.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Application;

/// <summary>
/// Get all categories for dropdown/filter in admin UI.
/// </summary>
public class GetCategories
{
    public record Query() : IRequest<List<CategoryDto>>
    {
        public string CacheKey => "categories_all";
        public TimeSpan? ExpirationSliding => TimeSpan.FromHours(1);
    }

    public class Handler(ICatalogUnitOfWork uow) : IRequestHandler<Query, List<CategoryDto>>
    {
        public async Task<List<CategoryDto>> Handle(Query query, CancellationToken cancellationToken)
        {
            var categories = await uow.Categories.AsQueryable()
                .OrderBy(c => c.Name)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name
            }).ToList();
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("api/categories", async (ISender sender) =>
            {
                var result = await sender.Send(new Query());
                return Results.Ok(result);
            })
            .WithTags("Categories")
            .WithName("GetCategories");
        }
    }
}
