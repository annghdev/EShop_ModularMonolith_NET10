using Contracts;

namespace Kernel.Application;

public interface IIntegrationEventPublisher
{
    Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default)
    where T : IntegrationEvent;
}
