using MediatR;
namespace Kernel;

public interface IInvalidatesCache
{
    public IEnumerable<string> CacheKeysToInvalidate { get; }
    public IEnumerable<string> CacheKeyPrefix { get; }
}

public interface ICommand : IRequest, IInvalidatesCache;

public interface ICommand<TResponse> : IRequest<TResponse>, IInvalidatesCache;