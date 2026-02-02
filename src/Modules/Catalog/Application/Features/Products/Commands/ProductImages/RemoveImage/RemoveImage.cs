using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Application;

/// <summary>
/// Remove an image from product gallery.
/// </summary>
public class RemoveImage
{
    public record Command(string Slug, string ImageUrl) : ICommand
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

            RuleFor(c => c.ImageUrl)
                .NotEmpty().WithMessage("Image URL cannot be empty");
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

            product.RemoveImage(new ImageUrl(command.ImageUrl));

            await uow.CommitAsync(cancellationToken);
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapDelete("api/products/{slug}/images", async (string slug, [Microsoft.AspNetCore.Mvc.FromQuery] string imageUrl, ISender sender) =>
            {
                await sender.Send(new Command(slug, imageUrl));
                return Results.NoContent();
            })
            .WithTags("Products")
            .WithName("RemoveProductImage")
            .RequireAuthorization();
        }
    }
}
