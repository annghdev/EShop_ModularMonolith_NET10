using Auth.Authentication;
using Contracts;
using Kernel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace API;

public class ReactAuthEndpoints : IEndpoint
{
    private const string RefreshTokenCookieName = "eshop_rt";

    public void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/react-auth")
            .WithTags("React Authentication");

        group.MapPost("/login", LoginAsync)
            .WithName("ReactLogin");

        group.MapPost("/register", RegisterAsync)
            .WithName("ReactRegister");

        group.MapPost("/refresh", RefreshAsync)
            .WithName("ReactRefresh");

        group.MapPost("/logout", LogoutAsync)
            .WithName("ReactLogout");
    }

    private static async Task<IResult> LoginAsync(LoginRequest request, ISender sender, HttpContext httpContext)
    {
        var authResult = await sender.Send(new Login.Command(request.Username, request.Password));
        if (string.IsNullOrWhiteSpace(authResult.RefreshToken))
        {
            return Results.Unauthorized();
        }

        SetRefreshCookie(httpContext, authResult.RefreshToken);
        return Results.Ok(new AuthResult(authResult.AccessToken, null) { UserInfo = authResult.UserInfo });
    }

    private static async Task<IResult> RegisterAsync(RegisterRequest request, ISender sender, HttpContext httpContext)
    {
        var authResult = await sender.Send(new RegisterAccountForCustomer.Command(
            request.Email,
            request.Password,
            request.PhoneNumber,
            request.FullName,
            request.GuestId));

        if (string.IsNullOrWhiteSpace(authResult.RefreshToken))
        {
            return Results.Unauthorized();
        }

        SetRefreshCookie(httpContext, authResult.RefreshToken);
        return Results.Created($"/users/{authResult.UserInfo?.DisplayName}", new AuthResult(authResult.AccessToken, null)
        {
            UserInfo = authResult.UserInfo
        });
    }

    private static async Task<IResult> RefreshAsync(ISender sender, HttpContext httpContext)
    {
        var refreshToken = httpContext.Request.Cookies[RefreshTokenCookieName];
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Results.Unauthorized();
        }

        var tokens = await sender.Send(new RefreshToken.Command(refreshToken));
        if (string.IsNullOrWhiteSpace(tokens.RefreshToken))
        {
            return Results.Unauthorized();
        }

        SetRefreshCookie(httpContext, tokens.RefreshToken);
        return Results.Ok(new TokensResult(tokens.AccessToken, null));
    }

    private static async Task<IResult> LogoutAsync(ISender sender, HttpContext httpContext)
    {
        var refreshToken = httpContext.Request.Cookies[RefreshTokenCookieName];
        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            await sender.Send(new Logout.Command(refreshToken));
        }

        ClearRefreshCookie(httpContext);
        return Results.Ok();
    }

    private static void SetRefreshCookie(HttpContext httpContext, string refreshToken)
    {
        httpContext.Response.Cookies.Append(RefreshTokenCookieName, refreshToken, BuildCookieOptions(httpContext, DateTimeOffset.UtcNow.AddDays(7)));
    }

    private static void ClearRefreshCookie(HttpContext httpContext)
    {
        httpContext.Response.Cookies.Delete(RefreshTokenCookieName, BuildCookieOptions(httpContext, DateTimeOffset.UtcNow.AddDays(-1)));
    }

    private static CookieOptions BuildCookieOptions(HttpContext httpContext, DateTimeOffset expires)
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = httpContext.Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            Expires = expires
        };
    }

    public record LoginRequest(string Username, string Password);

    public record RegisterRequest(
        string Email,
        string Password,
        string? PhoneNumber,
        string? FullName,
        string? GuestId);
}
