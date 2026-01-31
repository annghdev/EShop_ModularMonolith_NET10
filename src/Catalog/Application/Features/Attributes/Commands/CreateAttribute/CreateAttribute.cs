using Contracts.Requests.Catalog;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Application;

public class CreateAttribute
{
    public record Command(CreateAttributeRequest Request) : IRequest<Guid>
    {
        public IEnumerable<string> CacheKeysToInvalidate => ["attributes_all"];
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
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

    public class Handler(ICatalogUnitOfWork uow) : IRequestHandler<Command, Guid>
    {
        public async Task<Guid> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;
            var attribute = new Domain.Attribute
            {
                Id = Guid.CreateVersion7(),
                Name = request.Name,
                Icon = request.Icon,
                DisplayText = request.DisplayText,
                ValueStyleCss = request.ValueStyleCss
            };

            uow.Attributes.Add(attribute);
            await uow.CommitAsync(cancellationToken);
            return attribute.Id;
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("api/attributes", async (CreateAttributeRequest request, ISender sender) =>
            {
                var id = await sender.Send(new Command(request));
                return Results.Created($"api/attributes/{id}", new { id });
            })
            .WithTags("Attributes")
            .WithName("CreateAttribute")
            .RequireAuthorization();
        }
    }
}
