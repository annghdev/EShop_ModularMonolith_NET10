using Inventory.Domain;

namespace Inventory.Application;

public class InventoryMovementCreatedEventHandler(IInventoryUnitOfWork uow)
    : INotificationHandler<InventoryMovementCreatedEvent>
{
    public async Task Handle(InventoryMovementCreatedEvent notification, CancellationToken cancellationToken)
    {
        var movement = InventoryMovement.Create(
            notification.InventoryItemId,
            notification.WarehouseId,
            notification.ProductId,
            notification.VariantId,
            notification.Quantity,
            notification.Type,
            notification.SnapshotQuantity,
            notification.OrderId,
            notification.Reference);

        uow.InventoryMovements.Add(movement);
        await uow.CommitAsync(cancellationToken);
    }
}
