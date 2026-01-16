using Contracts;
using Kernel.Application;
using MassTransit;

namespace Infrastructure;

public class MassTransitEventHandler<T>(IIntegrationEventHandler<T> eventHandler) : IConsumer<T>
    where T : IntegrationEvent
{
    public async Task Consume(ConsumeContext<T> context)
    {
        await eventHandler.HandleAsync(context.Message);
    }
}
