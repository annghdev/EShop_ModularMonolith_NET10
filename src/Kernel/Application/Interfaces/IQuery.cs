using MediatR;

namespace Kernel;

public interface ICacheable
{
    public string CacheKey { get; }
    public TimeSpan? ExpirationSliding { get; }
}


public interface IQuery<T> : ICacheable, IRequest<T>;
