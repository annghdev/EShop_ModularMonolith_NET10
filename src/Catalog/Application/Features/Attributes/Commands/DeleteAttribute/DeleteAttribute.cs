using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Application;

public class DeleteAttribute
{
    public record Command(Guid Id) : IRequest
    {
        public IEnumerable<string> CacheKeysToInvalidate => ["attributes_all"];
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.Id)
                .NotEmpty().WithMessage("Attribute ID cannot be empty");
        }
    }

    public class Handler(ICatalogUnitOfWork uow) : IRequestHandler<Command>
    {
        public async Task Handle(Command command, CancellationToken cancellationToken)
        {
            var attribute = await uow.Attributes
                .FirstOrDefaultAsync(a => a.Id == command.Id && !a.IsDeleted, cancellationToken);

            if (attribute == null)
            {
                throw new NotFoundException("Attribute", command.Id);
            }

            attribute.IsDeleted = true;
            await uow.CommitAsync(cancellationToken);
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapDelete("api/attributes/{id:guid}", async (Guid id, ISender sender) =>
            {
                await sender.Send(new Command(id));
                return Results.NoContent();
            })
            .WithTags("Attributes")
            .WithName("DeleteAttribute")
            .RequireAuthorization();
        }
    }
}
