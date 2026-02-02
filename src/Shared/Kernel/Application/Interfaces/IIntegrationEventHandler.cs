using Contracts;

namespace Kernel.Application;

public interface IIntegrationEventHandler<T>
    where T : IntegrationEvent
{
    Task HandleAsync(T @event, CancellationToken cancellationToken = default);
}
