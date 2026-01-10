using Catalog.Domain;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Application;

public class RemoveVariant
{
    public record Request(Guid ProductId, Guid VariantId);

    public record Command(Request Request) : ICommand
    {
        public IEnumerable<string> CacheKeysToInvalidate => ["product_all"];
        public IEnumerable<string> CacheKeyPrefix => ["product"];
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.Request.ProductId)
                .NotEmpty().WithMessage("ProductId cannot be empty");

            RuleFor(c => c.Request.VariantId)
                .NotEmpty().WithMessage("VariantId cannot be empty");
        }
    }

    public class Handler(ICatalogUnitOfWork uow, IPublisher publisher) : IRequestHandler<Command>
    {
        public async Task Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            // Load the full product aggregate
            var product = await uow.Products.LoadFullAggregate(request.ProductId)
                ?? throw new NotFoundException("Product", request.ProductId);

            // Validate that the variant exists
            var variant = product.Variants.FirstOrDefault(v => v.Id == request.VariantId);
            if (variant == null)
                throw new NotFoundException("Variant", request.VariantId);

            // Business rule: Product must have at least one variant after removal
            if (product.Variants.Count <= 1)
                throw new DomainException("Product must have at least one variant");

            // Remove the variant from the product
            product.RemoveVariant(request.VariantId);

            // Commit the changes
            await uow.CommitAsync(cancellationToken);

            // Publish domain events (if any)
            // Note: Product.RemoveVariant doesn't seem to raise events, but we could add one if needed
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapDelete("api/products/{productId}/variants/{variantId}", async (Guid productId, Guid variantId, ISender sender) =>
            {
                var request = new Request(productId, variantId);
                await sender.Send(new Command(request));
                return Results.NoContent();
            })
                .WithName("RemoveVariant")
                .WithTags("Products");
        }
    }
}
