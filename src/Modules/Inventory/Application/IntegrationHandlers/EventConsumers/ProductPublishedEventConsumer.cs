using Contracts.IntegrationEvents.CatalogEvents;
using Inventory.Domain;
using Kernel.Domain;
using Microsoft.Extensions.Logging;

namespace Inventory.Application;

/// <summary>
/// Wolverine handler for ProductPublishedIntegrationEvent.
/// Automatically discovered by Wolverine due to naming convention (*Consumer).
/// </summary>
public class ProductPublishedEventConsumer
{
    public async Task Handle(
        ProductPublishedIntegrationEvent @event,
        IInventoryUnitOfWork uow,
        ILogger<ProductPublishedEventConsumer> logger,
        CancellationToken cancellationToken)
    {
        // Get ALL active warehouses
        var activeWarehouses = await uow.WarehouseRepository.GetActiveWarehousesAsync(cancellationToken);
        
        if (!activeWarehouses.Any())
        {
            logger.LogWarning("No active warehouses found. Creating default warehouse.");
            
            var defaultWarehouse = Warehouse.Create("WH-DEFAULT", "Default Warehouse", null, isDefault: true);
            uow.Warehouses.Add(defaultWarehouse);
            await uow.CommitAsync(cancellationToken);
            activeWarehouses = [defaultWarehouse];
        }

        logger.LogInformation("Creating inventory items for product {ProductId} ({ProductName}) in {WarehouseCount} warehouses",
            @event.ProductId, @event.ProductName, activeWarehouses.Count());

        // Add inventory items for each variant in EACH warehouse
        foreach (var warehouse in activeWarehouses)
        {
            foreach (var variant in @event.Variants)
            {
                // Check if already exists
                var existingItem = await uow.InventoryItemRepository.GetByVariantIdAsync(
                    variant.Id,
                    warehouse.Id,
                    cancellationToken);

                if (existingItem == null)
                {
                    var inventoryItem = InventoryItem.Create(
                        warehouse.Id,
                        @event.ProductId,
                        variant.Id,
                        new Sku(variant.Sku),
                        @event.ProductName, // Use ProductName from event
                        initialQuantity: 0,
                        lowStockThreshold: 5);

                    uow.InventoryItems.Add(inventoryItem);
                    
                    logger.LogDebug("Created inventory item for SKU {Sku} in warehouse {WarehouseCode}",
                        variant.Sku, warehouse.Code);
                }
            }
        }

        await uow.CommitAsync(cancellationToken);
    }
}
