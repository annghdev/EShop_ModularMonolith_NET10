using Contracts.Requests.Catalog;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Application;

public class UpdateAttributeValue
{
    public record Command(Guid AttributeId, Guid ValueId, UpdateAttributeValueRequest Request) : IRequest
    {
        public IEnumerable<string> CacheKeysToInvalidate => ["attributes_all"];
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.AttributeId)
                .NotEmpty().WithMessage("Attribute ID cannot be empty");

            RuleFor(c => c.ValueId)
                .NotEmpty().WithMessage("Value ID cannot be empty");

            RuleFor(c => c.Request.Value)
                .NotEmpty().WithMessage("Value cannot be empty")
                .MaximumLength(100).WithMessage("Value cannot exceed 100 characters");

            RuleFor(c => c.Request.ColorCode)
                .MaximumLength(7).WithMessage("Color code cannot exceed 7 characters")
                .When(c => !string.IsNullOrWhiteSpace(c.Request.ColorCode));
        }
    }

    public class Handler(ICatalogUnitOfWork uow) : IRequestHandler<Command>
    {
        public async Task Handle(Command command, CancellationToken cancellationToken)
        {
            var attribute = await uow.Attributes
                .Include(a => a.Values)
                .FirstOrDefaultAsync(a => a.Id == command.AttributeId && !a.IsDeleted, cancellationToken);

            if (attribute == null)
            {
                throw new NotFoundException("Attribute", command.AttributeId);
            }

            attribute.UpdateValue(command.ValueId, command.Request.Value, command.Request.ColorCode);
            await uow.CommitAsync(cancellationToken);
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPut("api/attributes/{id:guid}/values/{valueId:guid}", async (Guid id, Guid valueId, UpdateAttributeValueRequest request, ISender sender) =>
            {
                await sender.Send(new Command(id, valueId, request));
                return Results.NoContent();
            })
            .WithTags("Attributes")
            .WithName("UpdateAttributeValue")
            .RequireAuthorization();
        }
    }
}
