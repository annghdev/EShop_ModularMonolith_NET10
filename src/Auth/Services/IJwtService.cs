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
    Task<TokensResult> GenerateTokensAsync(Account account);

    /// <summary>
    /// Validate and parse a JWT token
    /// </summary>
    System.Security.Claims.ClaimsPrincipal? ValidateToken(string token);
}
