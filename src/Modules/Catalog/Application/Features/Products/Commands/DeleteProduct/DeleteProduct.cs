using Catalog.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Application.Features.Products.Commands.DeleteProduct;

public class DeleteProduct
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

            product.MarkAsDeleted();

            await uow.CommitAsync(cancellationToken);
        }
    }

    public class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapDelete("api/products/{id:guid}", async (Guid id, ISender sender) =>
            {
                await sender.Send(new Command(id));
                return Results.NoContent();
            })
                .WithName("DeleteProduct")
                .WithTags("Products");
        }
    }
}
