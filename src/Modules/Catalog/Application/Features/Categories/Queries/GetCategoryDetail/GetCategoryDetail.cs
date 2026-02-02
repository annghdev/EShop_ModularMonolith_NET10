using Contracts.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Application;

public class GetCategoryDetail
{
    public record Query(Guid Id) : IRequest<CategoryDetailDto>
    {
        public string CacheKey => $"category_detail_{Id}";
        public TimeSpan? ExpirationSliding => TimeSpan.FromHours(1);
    }

    public class Handler(ICatalogUnitOfWork uow) : IRequestHandler<Query, CategoryDetailDto>
    {
        public async Task<CategoryDetailDto> Handle(Query query, CancellationToken cancellationToken)
        {
            var category = await uow.Categories.GetByIdWithHierarchyAsync(query.Id, cancellationToken);
            if (category == null)
            {
                throw new NotFoundException("Category", query.Id);
            }

            var level = 0;
            var current = category.Parent;
            while (current != null)
            {
                level++;
                current = current.Parent;
            }

            var defaultAttributes = category.GetAllDefaultAttributesFromHierarchy()
                .Where(attr => attr.Attribute != null)
                .OrderBy(attr => attr.DisplayOrder)
                .Select(attr => new CategoryDefaultAttributeDto
                {
                    AttributeId = attr.AttributeId,
                    AttributeName = attr.Attribute!.Name,
                    DisplayOrder = attr.DisplayOrder
                })
                .ToList();

            var ownDefaultAttributes = category.DefaultAttributes
                .Where(attr => attr.Attribute != null)
                .OrderBy(attr => attr.DisplayOrder)
                .Select(attr => new CategoryDefaultAttributeDto
                {
                    AttributeId = attr.AttributeId,
                    AttributeName = attr.Attribute!.Name,
                    DisplayOrder = attr.DisplayOrder
                })
                .ToList();

            return new CategoryDetailDto
            {
                Id = category.Id,
                Name = category.Name,
                Image = category.Image,
                ParentId = category.ParentId,
                Level = level,
                DefaultAttributes = defaultAttributes,
                OwnDefaultAttributes = ownDefaultAttributes
            };
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("api/categories/{id:guid}", async (Guid id, ISender sender) =>
            {
                var result = await sender.Send(new Query(id));
                return Results.Ok(result);
            })
            .WithTags("Categories")
            .WithName("GetCategoryDetail");
        }
    }
}
