using Contracts;
using Contracts.Responses;
using FluentValidation;
using Kernel.Application;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Application;

/// <summary>
/// Get product details with inventory quantities by URL slug.
/// </summary>
public class GetProductDetailsBySlug
{
    public record Query(string Slug) : IRequest<ProductDetailsResponse>
    {
        public string CacheKey => $"product_details_slug:{Slug}";
        public TimeSpan? ExpirationSliding => TimeSpan.FromMinutes(15);
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

    public class Handler(
        ICatalogUnitOfWork uow,
        IMapper mapper,
        IIntegrationRequestSender requestSender) : IRequestHandler<Query, ProductDetailsResponse>
    {
        public async Task<ProductDetailsResponse> Handle(Query query, CancellationToken cancellationToken)
        {
            var product = await uow.Products.LoadFullAggregateBySlug(query.Slug, changeTracking: false);
            var productDto = mapper.Map<ProductDto>(product);

            List<VariantQuantityDto> variantQuantities = [];

            try
            {
                var inventoryResponse = await requestSender.SendQueryAsync<GetProductQuantityIntegrationQuery, ProductQuantityResponse>(
                    new GetProductQuantityIntegrationQuery("Catalog", product.Id),
                    cancellationToken);

                if (inventoryResponse?.Variants is not null)
                {
                    variantQuantities = inventoryResponse.Variants
                        .Select(v => new VariantQuantityDto(v.VariantId, v.Quantity))
                        .ToList();
                }
            }
            catch (NotFoundException)
            {
                variantQuantities = [];
            }

            return new ProductDetailsResponse
            {
                Product = productDto,
                VariantQuantities = variantQuantities
            };
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("api/products/details/{slug}", async (string slug, ISender sender) =>
            {
                var result = await sender.Send(new Query(slug));
                return Results.Ok(result);
            })
            .WithTags("Products")
            .WithName("GetProductDetailsBySlug");
        }
    }
}
