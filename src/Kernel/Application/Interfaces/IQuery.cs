using MediatR;

namespace Kernel.Application;

public interface IQuery
{
    public string CacheKey { get; }
    public TimeSpan? ExpirationSliding { get; }
}


public interface IQuery<T> : IQuery, IRequest<T>;
