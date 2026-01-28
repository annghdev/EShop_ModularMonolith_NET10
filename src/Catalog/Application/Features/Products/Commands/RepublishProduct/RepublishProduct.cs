using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Application;

/// <summary>
/// Republish a discontinued product back to Published status.
/// </summary>
public class RepublishProduct
{
    public record Command(Guid Id) : ICommand
    {
        public IEnumerable<string> CacheKeysToInvalidate => ["product_all", $"product_{Id}"];
        public IEnumerable<string> CacheKeyPrefix => ["product"];
    }

    public class Handler(ICatalogUnitOfWork uow) : IRequestHandler<Command>
    {
        public async Task Handle(Command command, CancellationToken cancellationToken)
        {
            var product = await uow.Products.LoadFullAggregate(command.Id);

            if (product == null)
            {
                throw new NotFoundException("Product", command.Id);
            }

            product.Republish();

            await uow.CommitAsync(cancellationToken);
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPut("api/products/{id:guid}/republish", async (Guid id, ISender sender) =>
            {
                await sender.Send(new Command(id));
                return Results.Ok();
            })
            .WithTags("Products")
            .WithName("RepublishProduct")
            .RequireAuthorization();
        }
    }
}
