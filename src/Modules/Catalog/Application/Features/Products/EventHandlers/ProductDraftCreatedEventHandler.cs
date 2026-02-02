using Catalog.Domain;

namespace Catalog.Application;

public class ProductDraftCreatedEventHandler : INotificationHandler<ProductDraftCreatedEvent>
{
    public Task Handle(ProductDraftCreatedEvent notification, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
