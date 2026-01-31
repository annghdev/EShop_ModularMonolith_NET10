using Contracts.Requests.Catalog;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Application;

public class UpdateAttribute
{
    public record Command(Guid Id, UpdateAttributeRequest Request) : IRequest
    {
        public IEnumerable<string> CacheKeysToInvalidate => ["attributes_all"];
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.Id)
                .NotEmpty().WithMessage("Attribute ID cannot be empty");

            RuleFor(c => c.Request.Name)
                .NotEmpty().WithMessage("Attribute name cannot be empty")
                .MaximumLength(100).WithMessage("Attribute name cannot exceed 100 characters");

            RuleFor(c => c.Request.Icon)
                .MaximumLength(500).WithMessage("Icon cannot exceed 500 characters")
                .When(c => !string.IsNullOrWhiteSpace(c.Request.Icon));

            RuleFor(c => c.Request.ValueStyleCss)
                .MaximumLength(1000).WithMessage("Value style cannot exceed 1000 characters")
                .When(c => !string.IsNullOrWhiteSpace(c.Request.ValueStyleCss));
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

            attribute.UpdateInfo(
                command.Request.Name,
                command.Request.Icon,
                command.Request.DisplayText,
                command.Request.ValueStyleCss
            );

            await uow.CommitAsync(cancellationToken);
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPut("api/attributes/{id:guid}", async (Guid id, UpdateAttributeRequest request, ISender sender) =>
            {
                await sender.Send(new Command(id, request));
                return Results.NoContent();
            })
            .WithTags("Attributes")
            .WithName("UpdateAttribute")
            .RequireAuthorization();
        }
    }
}
