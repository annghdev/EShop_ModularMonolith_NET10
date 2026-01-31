using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Application;

/// <summary>
/// Update product attribute settings.
/// </summary>
public class UpdateProductAttribute
{
    public record Command(string Slug, Guid AttributeId, bool HasVariant) : ICommand
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

            product.UpdateAttributeVariantUsage(command.AttributeId, command.HasVariant);

            await uow.CommitAsync(cancellationToken);
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPut("api/products/{slug}/attributes/{attributeId:guid}", async (string slug, Guid attributeId, UpdateProductAttributeRequest request, ISender sender) =>
            {
                await sender.Send(new Command(slug, attributeId, request.HasVariant));
                return Results.Ok();
            })
            .WithTags("Products")
            .WithName("UpdateProductAttribute")
            .RequireAuthorization();
        }
    }
}

public record UpdateProductAttributeRequest(bool HasVariant);
