using Kernel.Application;
using MediatR;

namespace Infrastructure;

public class IntegrationRequestSender(ISender sender) : IIntegrationRequestSender
{
    public Task SendCommandAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : IIntegrationCommand
    {
        return sender.Send(command, cancellationToken);
    }

    public  Task<TResponse> SendCommandAsync<TCommand, TResponse>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : IIntegrationCommand<TResponse>
    {
        return sender.Send(command, cancellationToken);
    }

    public Task<TResponse> SendQueryAsync<TQuery, TResponse>(TQuery query, CancellationToken cancellationToken = default) where TQuery : IIntegrationQuery<TResponse>
    {
        return sender.Send(query, cancellationToken);
    }
}
