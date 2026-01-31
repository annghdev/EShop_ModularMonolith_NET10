using Contracts.Requests.Catalog;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Application;

public class UpdateBrand
{
    public record Command(Guid Id, UpdateBrandRequest Request) : IRequest
    {
        public IEnumerable<string> CacheKeysToInvalidate => ["brands_all", $"brand_detail_{Id}"];
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.Id)
                .NotEmpty().WithMessage("Brand ID cannot be empty");

            RuleFor(c => c.Request.Name)
                .NotEmpty().WithMessage("Brand name cannot be empty")
                .MaximumLength(100).WithMessage("Brand name cannot exceed 100 characters");

            RuleFor(c => c.Request.Logo)
                .MaximumLength(500).WithMessage("Logo cannot exceed 500 characters")
                .When(c => !string.IsNullOrWhiteSpace(c.Request.Logo));
        }
    }

    public class Handler(ICatalogUnitOfWork uow) : IRequestHandler<Command>
    {
        public async Task Handle(Command command, CancellationToken cancellationToken)
        {
            var brand = await uow.Brands
                .FirstOrDefaultAsync(b => b.Id == command.Id, cancellationToken);

            if (brand == null)
            {
                throw new NotFoundException("Brand", command.Id);
            }

            brand.Name = command.Request.Name;
            brand.Logo = command.Request.Logo;

            await uow.CommitAsync(cancellationToken);
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPut("api/brands/{id:guid}", async (Guid id, UpdateBrandRequest request, ISender sender) =>
            {
                await sender.Send(new Command(id, request));
                return Results.NoContent();
            })
            .WithTags("Brands")
            .WithName("UpdateBrand")
            .RequireAuthorization();
        }
    }
}
