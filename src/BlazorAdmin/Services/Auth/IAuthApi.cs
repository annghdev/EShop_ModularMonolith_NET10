using Contracts;
using Refit;

namespace BlazorAdmin.Services.Auth;

/// <summary>
/// Refit interface for Auth module API endpoints
/// </summary>
public interface IAuthApi
{
    [Post("/auth/login")]
    Task<AuthResult> LoginAsync([Body] LoginCommand command);

    [Post("/auth/refresh")]
    Task<TokensResult> RefreshTokenAsync([Body] RefreshTokenCommand command);
}

public record LoginCommand(string Username, string Password);

public record RefreshTokenCommand(string RefreshToken);
