using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Application;

public class RemoveAttributeValue
{
    public record Command(Guid AttributeId, Guid ValueId) : IRequest
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
        }
    }

    public class Handler(ICatalogUnitOfWork uow) : IRequestHandler<Command>
    {
        public async Task Handle(Command command, CancellationToken cancellationToken)
        {
            var isUsed = await uow.VariantAttributeValues
                .AnyAsync(v => v.ValueId == command.ValueId, cancellationToken);
            if (isUsed)
            {
                throw new DomainException("Không thể xóa giá trị vì đang được sử dụng trong biến thể");
            }

            var attribute = await uow.Attributes
                .Include(a => a.Values)
                .FirstOrDefaultAsync(a => a.Id == command.AttributeId && !a.IsDeleted, cancellationToken);

            if (attribute == null)
            {
                throw new NotFoundException("Attribute", command.AttributeId);
            }

            attribute.RemoveValue(command.ValueId);
            await uow.CommitAsync(cancellationToken);
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapDelete("api/attributes/{id:guid}/values/{valueId:guid}", async (Guid id, Guid valueId, ISender sender) =>
            {
                await sender.Send(new Command(id, valueId));
                return Results.NoContent();
            })
            .WithTags("Attributes")
            .WithName("RemoveAttributeValue")
            .RequireAuthorization();
        }
    }
}
