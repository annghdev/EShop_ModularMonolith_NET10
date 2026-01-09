using Contracts.IntegrationEvents.CatalogEvents;
using Inventory.Domain;

namespace Inventory.Application;

public class ProductPublishedIntegrationEventHanler(IInventoryUnitOfWork uow)
    : INotificationHandler<ProductPublishedIntegrationEvent>
{
    public async Task Handle(ProductPublishedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        var stockItems = new List<StockItem>();
        foreach (var variant in notification.Payload)
        {
            stockItems.Add(StockItem.Create(variant.Name, new Sku(variant.Sku), 0));
        }

        uow.StockItems.AddRange(stockItems);
        await uow.CommitAsync(cancellationToken);
    }
}
