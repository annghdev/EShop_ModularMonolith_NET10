using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Application;

public class GetProductDetails
{
    public record Query(Guid Id) : IQuery<ProductDto>
    {
        public string CacheKey => $"product_{Id}";
        public TimeSpan? ExpirationSliding => TimeSpan.FromMinutes(30);
    }

    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(q => q.Id)
                .NotEmpty().WithMessage("Product ID cannot be empty");
        }
    }

    public class Handler(ICatalogUnitOfWork uow, IMapper mapper) : IRequestHandler<Query, ProductDto>
    {
        public async Task<ProductDto> Handle(Query query, CancellationToken cancellationToken)
        {
            var product = await uow.Products.GetAggregate(query.Id);

            return mapper.Map<ProductDto>(product);
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("api/products/{id}", async (Guid id, ISender sender) =>
            {
                var result = await sender.Send(new Query(id));
                return Results.Ok(result);
            })
            .WithTags("Products")
            .WithName("GetProductDetails");
        }
    }
}
