using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace BlazorAdmin.Auth;

/// <summary>
/// Custom authentication state provider that reads JWT from HttpOnly cookies.
/// Works with server-side Blazor by reading cookies via IHttpContextAccessor.
/// Supports chunked cookies for large JWT tokens.
/// </summary>
public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IJwtParser _jwtParser;
    private readonly ILogger<CustomAuthStateProvider> _logger;

    // Must match AuthController cookie names
    private const string AccessTokenCookie = ".jwt";

    public CustomAuthStateProvider(
        IHttpContextAccessor httpContextAccessor,
        IJwtParser jwtParser,
        ILogger<CustomAuthStateProvider> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _jwtParser = jwtParser;
        _logger = logger;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            // Log all cookies for debugging
            var cookies = _httpContextAccessor.HttpContext?.Request.Cookies;
            if (cookies != null)
            {
                _logger.LogInformation("Available cookies: {Cookies}", string.Join(", ", cookies.Keys));
            }
            else
            {
                _logger.LogWarning("No HttpContext or Cookies available");
            }
            
            var token = GetAccessToken();

            if (string.IsNullOrEmpty(token))
            {
                _logger.LogInformation("No access token found in cookies");
                return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
            }

            _logger.LogInformation("Access token found, length: {Length}", token.Length);

            // Check if token is expired
            if (_jwtParser.IsTokenExpired(token))
            {
                _logger.LogInformation("Access token is expired");
                return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
            }

            // Parse claims from JWT
            var claims = _jwtParser.ParseClaimsFromJwt(token);
            _logger.LogInformation("Parsed {ClaimCount} claims from token", claims.Count());
            
            if (!claims.Any())
            {
                _logger.LogWarning("No claims found in token");
                return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
            }

            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);

            _logger.LogInformation("User authenticated: {UserName}, IsAuthenticated: {IsAuth}", 
                user.Identity?.Name, user.Identity?.IsAuthenticated);
            return Task.FromResult(new AuthenticationState(user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting authentication state");
            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
        }
    }

    /// <summary>
    /// Get access token from cookies, supporting both regular and chunked cookies
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

    /// <summary>
    /// Notify that authentication state has changed (call after login/logout)
    /// </summary>
    public void NotifyUserAuthentication()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}