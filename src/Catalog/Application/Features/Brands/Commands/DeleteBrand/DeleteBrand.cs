using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Application;

public class DeleteBrand
{
    public record Command(Guid Id) : IRequest
    {
        public IEnumerable<string> CacheKeysToInvalidate => ["brands_all", $"brand_detail_{Id}"];
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.Id)
                .NotEmpty().WithMessage("Brand ID cannot be empty");
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

            brand.IsDeleted = true;
            await uow.CommitAsync(cancellationToken);
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapDelete("api/brands/{id:guid}", async (Guid id, ISender sender) =>
            {
                await sender.Send(new Command(id));
                return Results.NoContent();
            })
            .WithTags("Brands")
            .WithName("DeleteBrand")
            .RequireAuthorization();
        }
    }
}
