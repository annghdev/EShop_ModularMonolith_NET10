using Contracts.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Application;

public class GetProductDetails
{
    public record Query(Guid Id) : IRequest<ProductDto>;

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
            var product = await uow.Products.LoadFullAggregate(query.Id, changeTracking: false);

            var result = mapper.Map<ProductDto>(product);

            return result;
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
