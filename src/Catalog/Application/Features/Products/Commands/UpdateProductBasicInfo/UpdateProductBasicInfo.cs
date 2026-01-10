using Catalog.Domain;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Application;

public class UpdateProductBasicInfo
{
    public record Request
    {
        public Guid Id { get; init; }
        public string Name { get; init; }
        public string? Description { get; init; }
        public DimensionsDto Dimensions { get; init; }
    }

    public record Command(Request Request) : ICommand
    {
        public IEnumerable<string> CacheKeysToInvalidate => [$"product_{Request.Id}"];
        public IEnumerable<string> CacheKeyPrefix => ["product"];
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.Request.Id)
                .NotEmpty().WithMessage("Id cannot be empty");

            RuleFor(c => c.Request.Name)
                .NotEmpty().WithMessage("Name cannot be empty")
                .MaximumLength(200).WithMessage("Name cannot be greater than 200 characters");

            RuleFor(c => c.Request.Description)
                .MaximumLength(1000).WithMessage("Description cannot be greater than 1000 characters")
                .When(c => !string.IsNullOrEmpty(c.Request.Description));

            RuleFor(c => c.Request.Dimensions)
                .NotNull().WithMessage("Dimensions cannot be null")
                .Must(d => d.Width > 0).WithMessage("Width must be greater than 0")
                .Must(d => d.Height > 0).WithMessage("Height must be greater than 0")
                .Must(d => d.Depth > 0).WithMessage("Depth must be greater than 0")
                .Must(d => d.Weight > 0).WithMessage("Weight must be greater than 0");
        }
    }

    public class Handler(ICatalogUnitOfWork uow) : IRequestHandler<Command>
    {
        public async Task Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            var product = await uow.Products.LoadFullAggregate(request.Id)
                ?? throw new NotFoundException("Product", request.Id);

            product.UpdateBasicInfo(request.Name, request.Description, request.Dimensions.ToDimensions());

            await uow.CommitAsync(cancellationToken);
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPut("api/products/{id}/basic-info", async (Guid id, Request request, ISender sender) =>
            {
                request = request with { Id = id };
                await sender.Send(new Command(request));
                return Results.NoContent();
            })
                .WithName("UpdateProductBasicInfo")
                .WithTags("Products");
        }
    }
}
