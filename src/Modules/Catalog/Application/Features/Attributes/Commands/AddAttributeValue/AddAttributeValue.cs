using Contracts.Requests.Catalog;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Application;

public class AddAttributeValue
{
    public record Command(Guid AttributeId, AddAttributeValueRequest Request) : IRequest<Guid>
    {
        public IEnumerable<string> CacheKeysToInvalidate => ["attributes_all"];
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.AttributeId)
                .NotEmpty().WithMessage("Attribute ID cannot be empty");

            RuleFor(c => c.Request.Value)
                .NotEmpty().WithMessage("Value cannot be empty")
                .MaximumLength(100).WithMessage("Value cannot exceed 100 characters");

            RuleFor(c => c.Request.ColorCode)
                .MaximumLength(7).WithMessage("Color code cannot exceed 7 characters")
                .When(c => !string.IsNullOrWhiteSpace(c.Request.ColorCode));
        }
    }

    public class Handler(ICatalogUnitOfWork uow) : IRequestHandler<Command, Guid>
    {
        public async Task<Guid> Handle(Command command, CancellationToken cancellationToken)
        {
            var attribute = await uow.Attributes
                .Include(a => a.Values)
                .FirstOrDefaultAsync(a => a.Id == command.AttributeId && !a.IsDeleted, cancellationToken);

            if (attribute == null)
            {
                throw new NotFoundException("Attribute", command.AttributeId);
            }

            var value = attribute.AddValue(command.Request.Value, command.Request.ColorCode);
            await uow.CommitAsync(cancellationToken);
            return value.Id;
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("api/attributes/{id:guid}/values", async (Guid id, AddAttributeValueRequest request, ISender sender) =>
            {
                var valueId = await sender.Send(new Command(id, request));
                return Results.Created($"api/attributes/{id}/values/{valueId}", new { id = valueId });
            })
            .WithTags("Attributes")
            .WithName("AddAttributeValue")
            .RequireAuthorization();
        }
    }
}
