using Contracts.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Application;

public class GetCategoryDefaultAttributes
{
    public record Query(Guid CategoryId) : IQuery<List<CategoryDefaultAttributeDto>>
    {
        public string CacheKey => $"category_attributes_{CategoryId}";

        public TimeSpan? ExpirationSliding => TimeSpan.FromDays(7);
    }

    public class Handler(ICatalogUnitOfWork uow) : IRequestHandler<Query, List<CategoryDefaultAttributeDto>>
    {
        public async Task<List<CategoryDefaultAttributeDto>> Handle(Query query, CancellationToken cancellationToken)
        {
            var category = await uow.Categories.GetByIdWithHierarchyAsync(query.CategoryId, cancellationToken);
            if (category == null)
            {
                throw new NotFoundException("Category", query.CategoryId);
            }

            return category.GetAllDefaultAttributesFromHierarchy()
                .Where(attr => attr.Attribute != null)
                .OrderBy(attr => attr.DisplayOrder)
                .Select(attr => new CategoryDefaultAttributeDto
                {
                    AttributeId = attr.AttributeId,
                    AttributeName = attr.Attribute!.Name,
                    DisplayOrder = attr.DisplayOrder
                })
                .ToList();
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("api/categories/{categoryId}/default-attributes", async (Guid categoryId, ISender sender) =>
            {
                var result = await sender.Send(new Query(categoryId));
                return Results.Ok(result);
            })
            .WithTags("Categories")
            .WithName("GetCategoryDefaultAttributes");
        }
    }
}
