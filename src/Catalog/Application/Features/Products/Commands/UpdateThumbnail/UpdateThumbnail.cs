using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Application;

/// <summary>
/// Update product thumbnail image.
/// </summary>
public class UpdateThumbnail
{
    public record Command(string Slug, string ThumbnailUrl) : ICommand
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

            RuleFor(c => c.ThumbnailUrl)
                .NotEmpty().WithMessage("Thumbnail URL cannot be empty")
                .MaximumLength(500).WithMessage("Thumbnail URL cannot exceed 500 characters")
                .Must(BeValidUrl).WithMessage("Thumbnail must be a valid URL");
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

            product.UpdateThumbnail(new ImageUrl(command.ThumbnailUrl));

            await uow.CommitAsync(cancellationToken);
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPut("api/products/{slug}/thumbnail", async (string slug, UpdateThumbnailRequest request, ISender sender) =>
            {
                await sender.Send(new Command(slug, request.ThumbnailUrl));
                return Results.Ok();
            })
            .WithTags("Products")
            .WithName("UpdateProductThumbnail")
            .RequireAuthorization();
        }
    }
}

public record UpdateThumbnailRequest(string ThumbnailUrl);
