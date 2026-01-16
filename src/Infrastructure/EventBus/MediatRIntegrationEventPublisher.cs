using Contracts;
using Kernel.Application;
using MediatR;

namespace Infrastructure;

public sealed class MediatRIntegrationEventPublisher(IPublisher publisher) : IIntegrationEventPublisher
{
    public Task PublishAsync<T>(T @event, CancellationToken cancellationToken)
        where T : IntegrationEvent
    {
       return publisher.Publish(@event, cancellationToken);
    }
}
