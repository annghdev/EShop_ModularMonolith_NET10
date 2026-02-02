using Catalog.Domain;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Application;

public class AddVariant
{
    public record Request(
        Guid ProductId,
        string Name,
        string Sku,
        MoneyDto? OverrideCost,
        MoneyDto? OverridePrice,
        DimensionsDto? Dimensions,
        string? MainImage,
        IEnumerable<string> Images,
        Dictionary<Guid, Guid> AttributeValues);

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

            RuleFor(c => c.Request.Name)
                .NotEmpty().WithMessage("Variant name cannot be empty")
                .MaximumLength(200).WithMessage("Variant name cannot be greater than 200 characters");

            RuleFor(c => c.Request.Sku)
                .NotEmpty().WithMessage("Variant SKU cannot be empty")
                .MaximumLength(50).WithMessage("Variant SKU cannot be greater than 50 characters")
                .Matches(@"^[A-Za-z0-9\-_]+$").WithMessage("Variant SKU can only contain letters, numbers, hyphens, and underscores");

            RuleFor(c => c.Request.OverrideCost)
                .Must(cost => cost == null || cost.Amount >= 0)
                .WithMessage("Override cost amount must be greater than or equal to 0");

            RuleFor(c => c.Request.OverridePrice)
                .Must(price => price == null || price.Amount >= 0)
                .WithMessage("Override price amount must be greater than or equal to 0");

            RuleFor(c => c.Request.Dimensions)
                .Must(d => d == null || (d.Width > 0 && d.Height > 0 && d.Depth > 0 && d.Weight > 0))
                .WithMessage("Variant dimensions must have positive values when provided");

            RuleFor(c => c.Request.MainImage)
                .MaximumLength(500).WithMessage("Main image URL cannot be greater than 500 characters")
                .Must(BeValidUrl).WithMessage("Main image must be a valid URL")
                .When(c => !string.IsNullOrEmpty(c.Request.MainImage));

            RuleForEach(c => c.Request.Images)
                .MaximumLength(500).WithMessage("Variant image URL cannot be greater than 500 characters")
                .Must(BeValidUrl).WithMessage("Variant image must be a valid URL");

            RuleFor(c => c.Request.AttributeValues)
                .NotEmpty().WithMessage("Variant must have at least one attribute value")
                .Must(attrValues => attrValues.All(kvp => kvp.Key != Guid.Empty && kvp.Value != Guid.Empty))
                .WithMessage("All attribute values must have non-empty keys and values");
        }

        private static bool BeValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
                   && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
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

            // Validate that the variant SKU is unique within the product
            if (product.Variants.Any(v => v.Sku.Value == request.Sku))
                throw new DomainException($"Variant with SKU {request.Sku} already exists");

            // Prepare attribute values for the variant
            var attrs = new List<(ProductAttribute, Guid)>();
            foreach (var attr in product.Attributes)
            {
                if (!request.AttributeValues.ContainsKey(attr.AttributeId))
                    throw new DomainException($"Missing attribute value for attribute {attr.AttributeId}");

                attrs.Add((attr, request.AttributeValues[attr.AttributeId]));
            }

            // Create the variant
            ImageUrl? mainImage = null;
            if (!string.IsNullOrEmpty(request.MainImage))
            {
                mainImage = new ImageUrl(request.MainImage);
            }

            var variant = new Variant(
                request.Name,
                new Sku(request.Sku),
                request.OverrideCost?.ToMoney(),
                request.OverridePrice?.ToMoney(),
                mainImage,
                request.Dimensions?.ToDimensions(),
                attrs);

            // Add images to the variant
            foreach (var imageUrl in request.Images)
            {
                variant.AddImage(new ImageUrl(imageUrl), raiseEvent: false);
            }

            // Add the variant to the product
            product.AddVariant(variant, raiseEvent: true);

            // Commit the changes
            await uow.CommitAsync(cancellationToken);

            // Publish domain events
            await publisher.Publish(new VariantAddedEvent(variant));
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("api/products/{productId}/variants", async (Guid productId, Request request, ISender sender) =>
            {
                // Ensure the productId in the URL matches the one in the request
                if (productId != request.ProductId)
                    return Results.BadRequest("ProductId mismatch");

                await sender.Send(new Command(request));
                return Results.Accepted();
            })
                .WithName("AddVariant")
                .WithTags("Products");
        }
    }
}
