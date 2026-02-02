using Contracts.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Application;

/// <summary>
/// Get all attributes with their values for admin UI (variant creation, filtering).
/// </summary>
public class GetAttributes
{
    public record Query() : IRequest<List<AttributeDto>>
    {
        public string CacheKey => "attributes_all";
        public TimeSpan? ExpirationSliding => TimeSpan.FromHours(1);
    }

    public class Handler(ICatalogUnitOfWork uow) : IRequestHandler<Query, List<AttributeDto>>
    {
        public async Task<List<AttributeDto>> Handle(Query query, CancellationToken cancellationToken)
        {
            var attributes = await uow.Attributes
                .Where(a => !a.IsDeleted)
                .Include(a => a.Values)
                .OrderBy(a => a.Name)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return attributes.Select(a => new AttributeDto
            {
                Id = a.Id,
                Name = a.Name,
                Icon = a.Icon,
                ValueStyleCss = a.ValueStyleCss,
                DisplayText = a.DisplayText,
                Values = a.Values.Select(v => new AttributeValueDto
                {
                    Id = v.Id,
                    Value = v.Name,
                    ColorCode = v.ColorCode
                }).ToList()
            }).ToList();
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("api/attributes", async (ISender sender) =>
            {
                var result = await sender.Send(new Query());
                return Results.Ok(result);
            })
            .WithTags("Attributes")
            .WithName("GetAttributes");
        }
    }
}
