using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Application;

/// <summary>
/// Add an attribute to a product.
/// </summary>
public class AddProductAttribute
{
    public record Command(string Slug, Guid AttributeId, int DisplayOrder, bool HasVariant) : ICommand
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

            RuleFor(c => c.DisplayOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Display order must be >= 0");
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

            // Verify attribute exists
            var attribute = await uow.Attributes.FindAsync([command.AttributeId], cancellationToken);
            if (attribute == null)
            {
                throw new NotFoundException("Attribute", command.AttributeId);
            }

            // Get default value if any (first value of the attribute)
            var defaultValueId = attribute.Values.FirstOrDefault()?.Id ?? Guid.Empty;

            product.AddAttribute(command.AttributeId, defaultValueId, command.DisplayOrder, command.HasVariant);

            await uow.CommitAsync(cancellationToken);
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("api/products/{slug}/attributes", async (string slug, AddProductAttributeRequest request, ISender sender) =>
            {
                await sender.Send(new Command(slug, request.AttributeId, request.DisplayOrder, request.HasVariant));
                return Results.Created();
            })
            .WithTags("Products")
            .WithName("AddProductAttribute")
            .RequireAuthorization();
        }
    }
}

public record AddProductAttributeRequest(Guid AttributeId, int DisplayOrder, bool HasVariant = false);
