namespace Kernel.Application;

public interface IIntegrationRequestSender
{
    Task SendCommandAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : IIntegrationCommand;
    Task<TResponse> SendCommandAsync<TCommand, TResponse>(TCommand command, CancellationToken cancellationToken = default)
    where TCommand : IIntegrationCommand<TResponse>;

    Task<TResponse> SendQueryAsync<TQuery, TResponse>(TQuery query, CancellationToken cancellationToken = default)
        where TQuery : IIntegrationQuery<TResponse>;
}
