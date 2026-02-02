using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Application;

/// <summary>
/// Remove an attribute from a product.
/// </summary>
public class RemoveProductAttribute
{
    public record Command(string Slug, Guid AttributeId) : ICommand
    {
        public IEnumerable<string> CacheKeysToInvalidate => [$"product_slug_{Slug}"];
        public IEnumerable<string> CacheKeyPrefix => ["product"];
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.Slug)
                .NotEmpty().WithMessage("Slug cannot be empty");

            RuleFor(c => c.AttributeId)
                .NotEmpty().WithMessage("Attribute ID cannot be empty");
        }
    }

    public class Handler(ICatalogUnitOfWork uow) : IRequestHandler<Command>
    {
        public async Task Handle(Command command, CancellationToken cancellationToken)
        {
            var product = await uow.Products.LoadFullAggregateBySlug(command.Slug);

            if (product == null)
            {
                throw new NotFoundException("Product", command.Slug);
            }

            product.RemoveAttribute(command.AttributeId);

            await uow.CommitAsync(cancellationToken);
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapDelete("api/products/{slug}/attributes/{attributeId:guid}", async (string slug, Guid attributeId, ISender sender) =>
            {
                await sender.Send(new Command(slug, attributeId));
                return Results.NoContent();
            })
            .WithTags("Products")
            .WithName("RemoveProductAttribute")
            .RequireAuthorization();
        }
    }
}
