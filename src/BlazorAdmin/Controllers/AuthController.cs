using BlazorAdmin.Services.Auth;
using Microsoft.AspNetCore.Mvc;

namespace BlazorAdmin.Controllers;

/// <summary>
/// Server-side controller for cookie-based authentication operations.
/// Required because Blazor Server cannot directly set HttpOnly cookies.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthApi _authApi;
    private readonly ILogger<AuthController> _logger;

    // Use shorter cookie names
    private const string AccessTokenCookie = ".jwt";
    private const string RefreshTokenCookie = ".rt";
    private const string UserInfoCookie = ".ui";
    
    // Max cookie size (browsers limit to ~4096 bytes, leave room for overhead)
    private const int MaxCookieSize = 4000;

    public AuthController(IAuthApi authApi, ILogger<AuthController> logger)
    {
        _authApi = authApi;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand request)
    {
        try
        {
            var result = await _authApi.LoginAsync(request);

            _logger.LogInformation("Login successful. AccessToken length: {AccessLen}, RefreshToken length: {RefreshLen}",
                result.AccessToken?.Length ?? 0,
                result.RefreshToken?.Length ?? 0);

            // Check if token is too large for a single cookie
            if (!string.IsNullOrEmpty(result.AccessToken) && result.AccessToken.Length > MaxCookieSize)
            {
                _logger.LogWarning("AccessToken is too large ({Length} chars) for a single cookie. Consider reducing claims.", 
                    result.AccessToken.Length);
                
                // Split into chunks
                SetChunkedCookie(AccessTokenCookie, result.AccessToken, 60);
            }
            else
            {
                // Set HttpOnly cookies for tokens
                SetTokenCookie(AccessTokenCookie, result.AccessToken, 60);
            }
            
            SetTokenCookie(RefreshTokenCookie, result.RefreshToken, 60 * 24 * 7);

            // Set non-HttpOnly cookie for user info (needed for UI display)
            if (result.UserInfo != null)
            {
                var userInfoJson = System.Text.Json.JsonSerializer.Serialize(result.UserInfo);
                Response.Cookies.Append(UserInfoCookie, userInfoJson, new CookieOptions
                {
                    HttpOnly = false,
                    Secure = false,
                    SameSite = SameSiteMode.Lax,
                    Path = "/",
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                });
            }

            return Ok(new { Success = true, UserInfo = result.UserInfo });
        }
        catch (Refit.ApiException ex)
        {
            _logger.LogWarning(ex, "Login failed for user");
            return Unauthorized(new { Success = false, Message = "Tên đăng nhập hoặc mật khẩu không đúng" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during login");
            return StatusCode(500, new { Success = false, Message = "Đã có lỗi xảy ra" });
        }
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        // Clear all auth cookies (including chunks)
        Response.Cookies.Delete(AccessTokenCookie);
        Response.Cookies.Delete(RefreshTokenCookie);
        Response.Cookies.Delete(UserInfoCookie);
        
        // Clear any chunked cookies
        for (int i = 0; i < 10; i++)
        {
            Response.Cookies.Delete($"{AccessTokenCookie}_{i}");
        }

        return Ok(new { Success = true });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        try
        {
            var refreshToken = Request.Cookies[RefreshTokenCookie];
            if (string.IsNullOrEmpty(refreshToken))
            {
                return Unauthorized(new { Success = false, Message = "No refresh token" });
            }

            var result = await _authApi.RefreshTokenAsync(new RefreshTokenCommand(refreshToken));

            // Update cookies with new tokens
            if (!string.IsNullOrEmpty(result.AccessToken) && result.AccessToken.Length > MaxCookieSize)
            {
                SetChunkedCookie(AccessTokenCookie, result.AccessToken, 60);
            }
            else
            {
                SetTokenCookie(AccessTokenCookie, result.AccessToken, 60);
            }
            
            SetTokenCookie(RefreshTokenCookie, result.RefreshToken, 60 * 24 * 7);

            return Ok(new { Success = true });
        }
        catch (Refit.ApiException ex)
        {
            _logger.LogWarning(ex, "Token refresh failed");
            Response.Cookies.Delete(AccessTokenCookie);
            Response.Cookies.Delete(RefreshTokenCookie);
            Response.Cookies.Delete(UserInfoCookie);
            return Unauthorized(new { Success = false, Message = "Token refresh failed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during token refresh");
            return StatusCode(500, new { Success = false, Message = "Đã có lỗi xảy ra" });
        }
    }

    private void SetTokenCookie(string name, string? value, int expiresInMinutes)
    {
        if (string.IsNullOrEmpty(value)) return;
        
        var options = new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            Expires = DateTimeOffset.UtcNow.AddMinutes(expiresInMinutes)
        };

        Response.Cookies.Append(name, value, options);
    }
    
    private void SetChunkedCookie(string baseName, string value, int expiresInMinutes)
    {
        var options = new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            Expires = DateTimeOffset.UtcNow.AddMinutes(expiresInMinutes)
        };

        // Split into chunks
        int chunkSize = MaxCookieSize;
        int chunkCount = (int)Math.Ceiling((double)value.Length / chunkSize);
        
        // Store chunk count in base cookie
        Response.Cookies.Append(baseName, chunkCount.ToString(), options);
        
        for (int i = 0; i < chunkCount; i++)
        {
            int start = i * chunkSize;
            int length = Math.Min(chunkSize, value.Length - start);
            string chunk = value.Substring(start, length);
            Response.Cookies.Append($"{baseName}_{i}", chunk, options);
        }
    }
}
