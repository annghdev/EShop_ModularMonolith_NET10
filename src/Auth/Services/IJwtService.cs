using Contracts;

namespace Auth.Services;

/// <summary>
/// JWT token generation service interface
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Generate access and refresh tokens for an authenticated account
    /// </summary>
    Task<TokensResult> GenerateTokensAsync(Account account, string? createdByIp = null);

    /// <summary>
    /// Rotate refresh token and issue a new token pair.
    /// </summary>
    Task<TokensResult> RefreshTokensAsync(string refreshToken, string? createdByIp = null);

    /// <summary>
    /// Revoke a refresh token and prevent further use.
    /// </summary>
    Task RevokeRefreshTokenAsync(string refreshToken, string? revokedByIp = null);

    /// <summary>
    /// Validate and parse a JWT token
    /// </summary>
    System.Security.Claims.ClaimsPrincipal? ValidateToken(string token);
}
