using Contracts.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Application;

public class GetCategoryTree
{
    public record Query() : IRequest<List<CategoryTreeDto>>
    {
        public string CacheKey => "categories_tree";
        public TimeSpan? ExpirationSliding => TimeSpan.FromHours(1);
    }

    public class Handler(ICatalogUnitOfWork uow) : IRequestHandler<Query, List<CategoryTreeDto>>
    {
        public async Task<List<CategoryTreeDto>> Handle(Query query, CancellationToken cancellationToken)
        {
            var categories = await uow.Categories.AsQueryable()
                .AsNoTracking()
                .Select(c => new { c.Id, c.Name, c.ParentId })
                .ToListAsync(cancellationToken);

            var lookup = categories.ToDictionary(
                c => c.Id,
                c => new CategoryTreeDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    ParentId = c.ParentId
                });

            foreach (var category in categories)
            {
                if (category.ParentId.HasValue && lookup.TryGetValue(category.ParentId.Value, out var parent))
                {
                    parent.Children.Add(lookup[category.Id]);
                }
            }

            var roots = lookup.Values
                .Where(c => c.ParentId == null)
                .OrderBy(c => c.Name)
                .ToList();

            SortTree(roots);
            return roots;
        }

        private static void SortTree(List<CategoryTreeDto> nodes)
        {
            foreach (var node in nodes)
            {
                if (node.Children.Count > 0)
                {
                    node.Children = node.Children.OrderBy(c => c.Name).ToList();
                    SortTree(node.Children);
                }
            }
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("api/categories/tree", async (ISender sender) =>
            {
                var result = await sender.Send(new Query());
                return Results.Ok(result);
            })
            .WithTags("Categories")
            .WithName("GetCategoryTree");
        }
    }
}
