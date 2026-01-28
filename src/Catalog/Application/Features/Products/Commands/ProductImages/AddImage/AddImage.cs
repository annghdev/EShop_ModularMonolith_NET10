using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Application;

/// <summary>
/// Add an image to product gallery.
/// </summary>
public class AddImage
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
                .NotEmpty().WithMessage("Image URL cannot be empty")
                .MaximumLength(500).WithMessage("Image URL cannot exceed 500 characters")
                .Must(BeValidUrl).WithMessage("Image must be a valid URL");
        }

        private static bool BeValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
                   && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
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

            product.AddImage(new ImageUrl(command.ImageUrl));

            await uow.CommitAsync(cancellationToken);
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("api/products/{slug}/images", async (string slug, AddImageRequest request, ISender sender) =>
            {
                await sender.Send(new Command(slug, request.ImageUrl));
                return Results.Created();
            })
            .WithTags("Products")
            .WithName("AddProductImage")
            .RequireAuthorization();
        }
    }
}

public record AddImageRequest(string ImageUrl);
