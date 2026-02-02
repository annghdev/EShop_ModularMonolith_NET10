using System.Net.Http.Headers;

namespace BlazorAdmin.Auth;

/// <summary>
/// HTTP message handler that adds JWT Bearer token from cookies to outgoing requests.
/// </summary>
public class JwtAuthorizationHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<JwtAuthorizationHandler> _logger;
    
    private const string AccessTokenCookie = ".jwt";

    public JwtAuthorizationHandler(
        IHttpContextAccessor httpContextAccessor,
        ILogger<JwtAuthorizationHandler> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        var token = GetAccessToken();

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            _logger.LogInformation("Added Bearer token to request: {Method} {Url}", request.Method, request.RequestUri);
        }
        else
        {
            _logger.LogWarning("No token available for request: {Method} {Url}", request.Method, request.RequestUri);
        }

        var response = await base.SendAsync(request, cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("API request failed: {Method} {Url} -> {StatusCode}", 
                request.Method, request.RequestUri, response.StatusCode);
        }

        return response;
    }

    /// <summary>
    /// Get access token from cookies, supporting both regular and chunked cookies.
    /// Same logic as CustomAuthStateProvider.
    /// </summary>
    private string? GetAccessToken()
    {
        var cookies = _httpContextAccessor.HttpContext?.Request.Cookies;
        if (cookies == null) return null;

        // Try to get the base cookie
        if (!cookies.TryGetValue(AccessTokenCookie, out var baseValue))
        {
            return null;
        }

        // Check if it's a chunk count (numeric) or actual token
        if (int.TryParse(baseValue, out var chunkCount))
        {
            // It's chunked - reconstruct the token
            var chunks = new string[chunkCount];
            for (int i = 0; i < chunkCount; i++)
            {
                if (cookies.TryGetValue($"{AccessTokenCookie}_{i}", out var chunk))
                {
                    chunks[i] = chunk;
                }
                else
                {
                    _logger.LogWarning("Missing chunk {Index} for access token", i);
                    return null;
                }
            }
            return string.Concat(chunks);
        }
        
        // It's a regular (non-chunked) token
        return baseValue;
    }
}
