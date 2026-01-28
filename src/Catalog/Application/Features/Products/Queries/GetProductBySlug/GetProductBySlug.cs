using Contracts.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Application;

/// <summary>
/// Get product by URL slug for SEO-friendly URLs.
/// </summary>
public class GetProductBySlug
{
    public record Query(string Slug) : IQuery<ProductDto>
    {
        public string CacheKey => $"product_slug_{Slug}";
        public TimeSpan? ExpirationSliding => TimeSpan.FromMinutes(30);
    }

    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(q => q.Slug)
                .NotEmpty().WithMessage("Slug cannot be empty")
                .MaximumLength(200).WithMessage("Slug cannot exceed 200 characters");
        }
    }

    public class Handler(ICatalogUnitOfWork uow, IMapper mapper) : IRequestHandler<Query, ProductDto>
    {
        public async Task<ProductDto> Handle(Query query, CancellationToken cancellationToken)
        {
            var product = await uow.Products.LoadFullAggregateBySlug(query.Slug);

            if (product == null)
            {
                throw new NotFoundException("Product", query.Slug);
            }

            return mapper.Map<ProductDto>(product);
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("api/products/by-slug/{slug}", async (string slug, ISender sender) =>
            {
                var result = await sender.Send(new Query(slug));
                return Results.Ok(result);
            })
            .WithTags("Products")
            .WithName("GetProductBySlug");
        }
    }
}
