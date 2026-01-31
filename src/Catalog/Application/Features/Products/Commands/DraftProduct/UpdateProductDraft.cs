using Catalog.Domain;
using Contracts.Requests.Catalog;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Application;

/// <summary>
/// Update an existing product draft with basic info, pricing, and images.
/// </summary>
public class UpdateProductDraft
{
    public record Command(Guid ProductId, UpdateProductDraftRequest Request) : ICommand
    {
        public IEnumerable<string> CacheKeysToInvalidate => ["product_all", $"product_{ProductId}"];
        public IEnumerable<string> CacheKeyPrefix => ["product"];
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.ProductId)
                .NotEmpty().WithMessage("Product ID cannot be empty");

            RuleFor(c => c.Request.Name)
                .NotEmpty().WithMessage("Name cannot be empty")
                .MaximumLength(200).WithMessage("Name cannot exceed 200 characters");

            RuleFor(c => c.Request.Sku)
                .NotEmpty().WithMessage("SKU cannot be empty")
                .MaximumLength(50).WithMessage("SKU cannot exceed 50 characters");

            RuleFor(c => c.Request.CostAmount)
                .GreaterThanOrEqualTo(0).WithMessage("Cost must be >= 0");

            RuleFor(c => c.Request.PriceAmount)
                .GreaterThanOrEqualTo(0).WithMessage("Price must be >= 0");

            RuleFor(c => c.Request)
                .Must(r => r.PriceAmount >= r.CostAmount)
                .WithMessage("Price must be >= Cost");

            RuleFor(c => c.Request.Width)
                .GreaterThan(0).WithMessage("Width must be > 0");

            RuleFor(c => c.Request.Height)
                .GreaterThan(0).WithMessage("Height must be > 0");

            RuleFor(c => c.Request.Depth)
                .GreaterThan(0).WithMessage("Depth must be > 0");

            RuleFor(c => c.Request.Weight)
                .GreaterThan(0).WithMessage("Weight must be > 0");

            RuleFor(c => c.Request.CategoryId)
                .NotEmpty().WithMessage("Category ID cannot be empty");

            RuleFor(c => c.Request.BrandId)
                .NotEmpty().WithMessage("Brand ID cannot be empty");
        }
    }

    public class Handler(ICatalogUnitOfWork uow) : IRequestHandler<Command>
    {
        public async Task Handle(Command command, CancellationToken cancellationToken)
        {
            var product = await uow.Products.LoadFullAggregate(command.ProductId);

            if (product == null)
            {
                throw new NotFoundException("Product", command.ProductId);
            }

            if (product.Status != ProductStatus.Draft)
            {
                throw new DomainException("Only draft products can be updated via this endpoint");
            }

            var request = command.Request;

            // Update basic info
            product.UpdateBasicInfo(
                request.Name,
                request.Description,
                new Dimensions(request.Width, request.Height, request.Depth, request.Weight)
            );

            // Update pricing
            product.UpdatePricing(
                new Money(request.CostAmount),
                new Money(request.PriceAmount)
            );

            // Update category
            if (product.CategoryId != request.CategoryId)
            {
                product.SetCategory(request.CategoryId);
            }

            // Update brand
            if (product.BrandId != request.BrandId)
            {
                product.SetBrand(request.BrandId);
            }

            // Update thumbnail
            if (!string.IsNullOrEmpty(request.Thumbnail))
            {
                product.UpdateThumbnail(new ImageUrl(request.Thumbnail), raiseEvent: false);
            }

            // Ensure category default attributes exist on draft
            var category = await uow.Categories.GetByIdWithHierarchyAsync(request.CategoryId, cancellationToken)
                ?? throw new NotFoundException("Category", request.CategoryId);

            foreach (var defaultAttr in category.GetAllDefaultAttributesFromHierarchy())
            {
                if (product.Attributes.Any(a => a.AttributeId == defaultAttr.AttributeId))
                {
                    continue;
                }

                var attribute = await uow.Attributes.FindAsync([defaultAttr.AttributeId], cancellationToken);
                if (attribute == null)
                {
                    throw new NotFoundException("Attribute", defaultAttr.AttributeId);
                }

                var defaultValueId = attribute.Values.FirstOrDefault()?.Id ?? Guid.Empty;
                product.AddAttribute(defaultAttr.AttributeId, defaultValueId, defaultAttr.DisplayOrder, false);
            }

            await uow.CommitAsync(cancellationToken);
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPut("api/admin/products/{id:guid}/draft", async (Guid id, UpdateProductDraftRequest request, ISender sender) =>
            {
                await sender.Send(new Command(id, request));
                return Results.Ok();
            })
            .WithTags("Admin Products")
            .WithName("UpdateProductDraft")
            .RequireAuthorization();
        }
    }
}
