using Catalog.Domain;
using Elastic.Clients.Elasticsearch;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Application;

public class UpdateVariant
{
    public record Request(
        Guid ProductId,
        Guid VariantId,
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
        public IEnumerable<string> CacheKeysToInvalidate => ["product_all", $"product_{Request.ProductId}"];
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

            // Validate that the variant exists
            var variant = product.Variants.FirstOrDefault(v => v.Id == request.VariantId);
            if (variant == null)
                throw new NotFoundException("Variant", request.VariantId);

            // Validate that the new SKU is unique (if it's different from the current one)
            if (variant.Sku.Value != request.Sku &&
                product.Variants.Any(v => v.Sku.Value == request.Sku && v.Id != request.VariantId))
                throw new DomainException($"Variant with SKU {request.Sku} already exists");

            // Update basic info (name and SKU)
            variant.UpdateBasicInfo(request.Name, new Sku(request.Sku));

            // Update pricing if provided
            if (request.OverrideCost != null || request.OverridePrice != null)
            {
                var cost = request.OverrideCost?.ToMoney() ?? variant.OverrideCost;
                var price = request.OverridePrice?.ToMoney() ?? variant.OverridePrice;

                if (cost != null && price != null)
                {
                    variant.UpdatePricing(cost, price);
                }
            }

            // Update dimensions
            variant.UpdateDimensions(request.Dimensions?.ToDimensions());

            // Update main image
            if (!string.IsNullOrEmpty(request.MainImage))
            {
                variant.SetMainImage(new ImageUrl(request.MainImage), raiseEvent: false);
            }
            else
            {
                // Clear main image if not provided
                variant.SetMainImage(null, raiseEvent: false);
            }

            // Update images
            variant.ClearImages();
            foreach (var imageUrl in request.Images)
            {
                variant.AddImage(new ImageUrl(imageUrl), raiseEvent: false);
            }

            // Update attribute values
            var attrs = new List<(ProductAttribute, Guid)>();
            foreach (var attr in product.Attributes)
            {
                if (!request.AttributeValues.ContainsKey(attr.AttributeId))
                    throw new DomainException($"Missing attribute value for attribute {attr.AttributeId}");

                attrs.Add((attr, request.AttributeValues[attr.AttributeId]));
            }
            variant.UpdateAttributeValues(attrs);

            // Commit the changes
            await uow.CommitAsync(cancellationToken);

            // Publish domain events
            await publisher.Publish(new VariantUpdatedEvent(variant));
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPut("api/products/{productId}/variants/{variantId}", async (Guid productId, Guid variantId, Request request, ISender sender) =>
            {
                // Ensure the productId and variantId in the URL match the ones in the request
                if (productId != request.ProductId || variantId != request.VariantId)
                    return Results.BadRequest("ProductId or VariantId mismatch");

                await sender.Send(new Command(request));
                return Results.Accepted();
            })
                .WithName("UpdateVariant")
                .WithTags("Products");
        }
    }
}
