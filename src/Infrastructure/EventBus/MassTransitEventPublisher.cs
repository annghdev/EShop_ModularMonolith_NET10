using Contracts;
using Kernel.Application;
using MassTransit;

namespace Infrastructure;

public class MasstransitEventPublisher(IPublishEndpoint publisher) : IIntegrationEventPublisher
{
    public Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default)
        where T : IntegrationEvent
    {
        return publisher.Publish(@event, cancellationToken);
    }
}
