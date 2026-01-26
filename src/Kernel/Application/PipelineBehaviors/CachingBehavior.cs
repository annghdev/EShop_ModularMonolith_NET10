using MediatR;
using Microsoft.Extensions.Logging;

namespace Kernel.Application;

public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : class
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;

    public CachingBehavior(
        ICacheService cacheService,
        ILogger<CachingBehavior<TRequest, TResponse>> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Only process cacheable requests
        if (request is not ICacheable query)
        {
            return await next();
        }

        // Generate cache key
        var cacheKey = query.CacheKey;
        if (string.IsNullOrEmpty(cacheKey))
        {
            return await next();
        }

        _logger.LogDebug("Checking cache for key: {CacheKey}", cacheKey);

        // Try to get from cache
        var cachedResponse = await _cacheService.GetAsync<TResponse>(cacheKey, cancellationToken);

        if (cachedResponse != null)
        {
            _logger.LogDebug("Cache hit for key: {CacheKey}", cacheKey);
            return cachedResponse;
        }

        _logger.LogDebug("Cache miss for key: {CacheKey}", cacheKey);

        // Execute the request handler
        var response = await next();

        // Cache the response
        await _cacheService.SetAsync(
            cacheKey,
            response,
            query.ExpirationSliding,
            cancellationToken);

        _logger.LogDebug("Cached response for key: {CacheKey}", cacheKey);

        return response;
    }
}