using Catalog.Domain;
using Contracts.Requests.Catalog;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Application;

public class CreateBrand
{
    public record Command(CreateBrandRequest Request) : IRequest<Guid>
    {
        public IEnumerable<string> CacheKeysToInvalidate => ["brands_all"];
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.Request.Name)
                .NotEmpty().WithMessage("Brand name cannot be empty")
                .MaximumLength(100).WithMessage("Brand name cannot exceed 100 characters");

            RuleFor(c => c.Request.Logo)
                .MaximumLength(500).WithMessage("Logo cannot exceed 500 characters")
                .When(c => !string.IsNullOrWhiteSpace(c.Request.Logo));
        }
    }

    public class Handler(ICatalogUnitOfWork uow) : IRequestHandler<Command, Guid>
    {
        public async Task<Guid> Handle(Command command, CancellationToken cancellationToken)
        {
            var brand = new Brand
            {
                Id = Guid.CreateVersion7(),
                Name = command.Request.Name,
                Logo = command.Request.Logo
            };

            uow.Brands.Add(brand);
            await uow.CommitAsync(cancellationToken);
            return brand.Id;
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("api/brands", async (CreateBrandRequest request, ISender sender) =>
            {
                var id = await sender.Send(new Command(request));
                return Results.Created($"api/brands/{id}", new { id });
            })
            .WithTags("Brands")
            .WithName("CreateBrand")
            .RequireAuthorization();
        }
    }
}
