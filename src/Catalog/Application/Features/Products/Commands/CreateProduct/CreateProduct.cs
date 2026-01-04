using Catalog.Domain;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Application;

public class CreateProduct
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
            RuleFor(c => c.Request.Name)
                .NotEmpty().WithMessage("Name cannot be empty")
                .MaximumLength(200).WithMessage("Name cannot greater than 200 charactors");
        }
    }

    public class Handler(ICatalogUnitOfWork uow, IPublisher publisher) : IRequestHandler<Command>
    {
        public async Task Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            var category = await uow.Categories
                .FirstOrDefaultAsync(c => c.Id == request.CategoryId, cancellationToken)
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
