using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Application;

public class DiscardProduct
{
    public record Command(Guid Id) : ICommand
    {
        public IEnumerable<string> CacheKeysToInvalidate => ["product_all"];
        public IEnumerable<string> CacheKeyPrefix => ["product"];
    }

    public class Handler(ICatalogUnitOfWork uow) : IRequestHandler<Command>
    {
        public async Task Handle(Command command, CancellationToken cancellationToken)
        {
            var product = await uow.Products.LoadFullAggregate(command.Id);

            product.Discard();

            await uow.CommitAsync(cancellationToken);
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapDelete("api/products/{id:guid}/discard", async (Guid id, ISender sender) =>
            {
                await sender.Send(new Command(id));
            })
                .WithName("DiscardProduct")
                .WithTags("Products");
        }
    }
}
