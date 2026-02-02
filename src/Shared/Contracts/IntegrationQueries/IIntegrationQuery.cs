using MediatR;

namespace Contracts;

public interface IIntegrationQuery<TResponse> : IRequest<TResponse>
{
    string CallFrom { get; }
}