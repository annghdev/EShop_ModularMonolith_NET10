using Contracts;
using Kernel.Application;
using MediatR;

namespace Kernel.Infrastructure.EventBus;

public sealed class IntegrationEventPublisher(IPublisher publisher) : IIntegrationEventPublisher
{
    public Task PublishAsync<T>(T @event, CancellationToken cancellationToken)
        where T : IntegrationEvent
    {
       return publisher.Publish(@event, cancellationToken);
    }
}
