using MediatR;

namespace Contracts;

public interface IIntegrationCommand : IRequest
{
    string CallFrom { get; }
}

public interface IIntegrationCommand<TResponse> : IRequest<TResponse>
{
    string CallFrom { get; }
}