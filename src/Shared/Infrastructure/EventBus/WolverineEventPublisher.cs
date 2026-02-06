using Contracts;
using Kernel.Application;
using Wolverine;

namespace Infrastructure;

public class WolverineEventPublisher(IMessageBus messageBus) : IIntegrationEventPublisher
{
    public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default)
        where T : IntegrationEvent
    {
        await messageBus.PublishAsync(@event);
    }
}
