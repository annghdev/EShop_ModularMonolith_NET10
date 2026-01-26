using Contracts.IntegrationEvents.CatalogEvents;
using Inventory.Domain;

namespace Inventory.Application;

public class ProductPublishedIntegrationEventHandler(IInventoryUnitOfWork uow)
    : IIntegrationEventHandler<ProductPublishedIntegrationEvent>
{
    public async Task HandleAsync(ProductPublishedIntegrationEvent @event, CancellationToken cancellationToken = default)
    {
        // Get or create default warehouse
        var defaultWarehouse = await uow.WarehouseRepository.GetDefaultWarehouseAsync(cancellationToken);
        
        if (defaultWarehouse == null)
        {
            // Create default warehouse if not exists
            defaultWarehouse = Warehouse.Create("DEFAULT", "Default Warehouse", null, isDefault: true);
            uow.Warehouses.Add(defaultWarehouse);
            await uow.CommitAsync(cancellationToken); // Save to get the warehouse ID
        }

        // Add inventory items for each variant
        foreach (var variant in @event.Payload)
        {
            // Check if already exists
            var existingItem = await uow.InventoryItemRepository.GetByVariantIdAsync(
                variant.Id, 
                defaultWarehouse.Id, 
                cancellationToken);

            if (existingItem == null)
            {
                var inventoryItem = InventoryItem.Create(
                    defaultWarehouse.Id,
                    @event.ProductId,
                    variant.Id,
                    new Sku(variant.Sku),
                    initialQuantity: 0,
                    lowStockThreshold: 5);

                uow.InventoryItems.Add(inventoryItem);
            }
        }

        await uow.CommitAsync(cancellationToken);
    }
}
