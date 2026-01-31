using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Application;

public class PublishProduct
{
    public record Command(Guid Id) : ICommand
    {
        public IEnumerable<string> CacheKeysToInvalidate => ["product_all", $"product_{Id}"];
        public IEnumerable<string> CacheKeyPrefix => ["product", "product_slug"];
    }

    public class Handler(ICatalogUnitOfWork uow) : IRequestHandler<Command>
    {
        public async Task Handle(Command command, CancellationToken cancellationToken)
        {
            var product = await uow.Products.LoadFullAggregate(command.Id);

            product.Publish();

            //uow.Products.Update(product);

            await uow.CommitAsync(cancellationToken);
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPut("api/products/publish/{id:guid}", async (Guid id, ISender sender) =>
            {
                await sender.Send(new Command(id));
                return Results.Ok();
            })
                .WithName("PublishProduct")
                .WithTags("Products");
        }
    }
}
