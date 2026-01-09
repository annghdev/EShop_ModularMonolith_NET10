using Catalog.Domain;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Application;

public class DraftProduct
{
    public record Request
    {
        public Guid Id { get; init; }
        public string Name { get; init; }
        public string? Description { get; init; }
        public string Sku { get; init; }
        public MoneyDto Cost { get; init; }
        public MoneyDto Price { get; init; }
        public DimensionsDto Dimensions { get; init; }
        public bool HasStockQuantity { get; init; }
        public Guid CategoryId { get; init; }
        public Guid BrandId { get; init; }
        public int DisplayPriority { get; set; }
        public string? Thumbnail { get; set; }
        public IEnumerable<string> Images { get; init; } = [];
        public IEnumerable<AddProductAttributeDto> Attributes { get; init; } = [];
        public IEnumerable<AddVariantDto> Variants { get; init; } = [];
    }

    public record AddVariantDto(
        string Name,
        string Sku,
        MoneyDto OverrideCost,
        MoneyDto OverridePrice,
        DimensionsDto Dimensions,
        string? MainImage,
        IEnumerable<string> Images,
        Dictionary<Guid, Guid> AttributeValues);

    public record AddProductAttributeDto(
        Guid AttributeId,
        Guid Value,
        int DisplayOrder);

    public record Command(Request Request) : ICommand
    {
        public IEnumerable<string> CacheKeysToInvalidate => ["product_all"];
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

            RuleFor(c => c.Request.Sku)
                .NotEmpty().WithMessage("SKU cannot be empty")
                .MaximumLength(50).WithMessage("SKU cannot be greater than 50 characters")
                .Matches(@"^[A-Za-z0-9\-_]+$").WithMessage("SKU can only contain letters, numbers, hyphens, and underscores");

            RuleFor(c => c.Request.Cost)
                .NotNull().WithMessage("Cost cannot be null")
                .Must(cost => cost.Amount >= 0).WithMessage("Cost amount must be greater than or equal to 0");

            RuleFor(c => c.Request.Price)
                .NotNull().WithMessage("Price cannot be null")
                .Must(price => price.Amount >= 0).WithMessage("Price amount must be greater than or equal to 0")
                .Must((command, price) => price.Amount >= command.Request.Cost.Amount)
                .WithMessage("Price amount must be greater than or equal to cost amount");

            RuleFor(c => c.Request.Dimensions)
                .NotNull().WithMessage("Dimensions cannot be null")
                .Must(d => d.Width > 0).WithMessage("Width must be greater than 0")
                .Must(d => d.Height > 0).WithMessage("Height must be greater than 0")
                .Must(d => d.Depth > 0).WithMessage("Depth must be greater than 0")
                .Must(d => d.Weight > 0).WithMessage("Weight must be greater than 0");

            RuleFor(c => c.Request.CategoryId)
                .NotEmpty().WithMessage("CategoryId cannot be empty");

            RuleFor(c => c.Request.BrandId)
                .NotEmpty().WithMessage("BrandId cannot be empty");

            RuleFor(c => c.Request.DisplayPriority)
                .GreaterThanOrEqualTo(0).WithMessage("DisplayPriority must be greater than or equal to 0");

            RuleFor(c => c.Request.Thumbnail)
                .MaximumLength(500).WithMessage("Thumbnail URL cannot be greater than 500 characters")
                .Must(BeValidUrl).WithMessage("Thumbnail must be a valid URL")
                .When(c => !string.IsNullOrEmpty(c.Request.Thumbnail));

            RuleForEach(c => c.Request.Images)
                .NotEmpty().WithMessage("Image URL cannot be empty")
                .MaximumLength(500).WithMessage("Image URL cannot be greater than 500 characters")
                .Must(BeValidUrl).WithMessage("Image must be a valid URL");

            RuleForEach(c => c.Request.Attributes)
                .Must(attr => attr.AttributeId != Guid.Empty).WithMessage("AttributeId cannot be empty")
                .Must(attr => attr.Value != Guid.Empty).WithMessage("Attribute value cannot be empty")
                .Must(attr => attr.DisplayOrder >= 0).WithMessage("Attribute display order must be greater than or equal to 0");

            RuleForEach(c => c.Request.Variants)
                .ChildRules(variant =>
                {
                    variant.RuleFor(v => v.Name)
                        .NotEmpty().WithMessage("Variant name cannot be empty")
                        .MaximumLength(200).WithMessage("Variant name cannot be greater than 200 characters");

                    variant.RuleFor(v => v.Sku)
                        .NotEmpty().WithMessage("Variant SKU cannot be empty")
                        .MaximumLength(50).WithMessage("Variant SKU cannot be greater than 50 characters")
                        .Matches(@"^[A-Za-z0-9\-_]+$").WithMessage("Variant SKU can only contain letters, numbers, hyphens, and underscores");

                    variant.RuleFor(v => v.OverrideCost)
                        .Must(cost => cost == null || cost.Amount >= 0)
                        .WithMessage("Override cost amount must be greater than or equal to 0");

                    variant.RuleFor(v => v.OverridePrice)
                        .Must(price => price == null || price.Amount >= 0)
                        .WithMessage("Override price amount must be greater than or equal to 0");

                    variant.RuleFor(v => v.Dimensions)
                        .Must(d => d == null || (d.Width > 0 && d.Height > 0 && d.Depth > 0 && d.Weight > 0))
                        .WithMessage("Variant dimensions must have positive values when provided");

                    variant.RuleFor(v => v.MainImage)
                        .MaximumLength(500).WithMessage("Main image URL cannot be greater than 500 characters")
                        .Must(BeValidUrl).WithMessage("Main image must be a valid URL")
                        .When(v => !string.IsNullOrEmpty(v.MainImage));

                    variant.RuleForEach(v => v.Images)
                        .MaximumLength(500).WithMessage("Variant image URL cannot be greater than 500 characters")
                        .Must(BeValidUrl).WithMessage("Variant image must be a valid URL");

                    variant.RuleFor(v => v.AttributeValues)
                        .NotEmpty().WithMessage("Variant must have at least one attribute value")
                        .Must(attrValues => attrValues.All(kvp => kvp.Key != Guid.Empty && kvp.Value != Guid.Empty))
                        .WithMessage("All attribute values must have non-empty keys and values");
                });

            // Business rule: Variant attributeValues must match product attributes
            RuleFor(c => c.Request)
                .Must(request =>
                {
                    if (!request.Attributes.Any() || !request.Variants.Any())
                        return true; // Skip validation if no attributes or variants

                    var productAttributeIds = request.Attributes.Select(a => a.AttributeId).ToHashSet();

                    return request.Variants.All(variant =>
                    {
                        // Each variant must have attributeValues for all product attributes
                        return productAttributeIds.All(attrId => variant.AttributeValues.ContainsKey(attrId)) &&
                               productAttributeIds.Count == variant.AttributeValues.Count;
                    });
                })
                .WithMessage("Each variant must have attribute values for all product attributes");

            // Business rule: At least one variant is required
            RuleFor(c => c.Request.Variants)
                .Must(variants => variants.Any()).WithMessage("At least one variant is required");
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

            // Load category with full parent hierarchy to validate default attributes
            var category = await uow.Categories.GetByIdWithHierarchyAsync(request.CategoryId, cancellationToken)
                ?? throw new NotFoundException("Category", request.CategoryId);

            var buider = ProductBuilder.Draft(
                    request.Id,
                    request.Name,
                    request.Description,
                    new Sku(request.Sku),
                    request.Cost.ToMoney(),
                    request.Price.ToMoney(),
                    request.Dimensions.ToDimensions(),
                    request.HasStockQuantity,
                    category,
                    request.BrandId
                )
                .SetThumbnail(request.Thumbnail)
                .SetImages(request.Images);

            foreach (var attr in request.Attributes)
            {
                buider.AddAttribute(attr.AttributeId, attr.Value, attr.DisplayOrder);
            }

            foreach (var item in request.Variants)
            {
                buider.AddVariant(
                    item.Name,
                    new Sku(item.Sku),
                    item.AttributeValues,
                    item.MainImage,
                    item.Images,
                    item.OverrideCost?.ToMoney(),
                    item.OverridePrice?.ToMoney(),
                    item.Dimensions?.ToDimensions()
                    );
            }

            var product = buider.Build();

            uow.Products.Add(product);
            await uow.CommitAsync(cancellationToken);

            await publisher.Publish(new ProductDraftCreatedEvent(product));
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("api/products", async (Request request, ISender sender) =>
            {
                await sender.Send(new Command(request));
                return Results.Accepted();
            });
        }
    }

}
