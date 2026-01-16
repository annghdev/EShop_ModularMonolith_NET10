using Contracts.IntegrationEvents.CatalogEvents;
using Inventory.Domain;

namespace Inventory.Application;

public class ProductPublishedIntegrationEventHanler(IInventoryUnitOfWork uow)
    : IIntegrationEventHandler<ProductPublishedIntegrationEvent>
{
    public async Task HandleAsync(ProductPublishedIntegrationEvent @event, CancellationToken cancellationToken = default)
    {
        var stockItems = new List<StockItem>();

        foreach (var variant in @event.Payload)
        {
            stockItems.Add(StockItem.Create(variant.Name, new Sku(variant.Sku), 0));
        }

        uow.StockItems.AddRange(stockItems);
        await uow.CommitAsync(cancellationToken);
    }
}
