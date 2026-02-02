using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BlazorAdmin.Auth;

/// <summary>
/// Utility to parse JWT tokens and extract claims
/// </summary>
public interface IJwtParser
{
    IEnumerable<Claim> ParseClaimsFromJwt(string jwt);
    bool IsTokenExpired(string jwt);
    DateTime? GetTokenExpiration(string jwt);
}

public class JwtParser : IJwtParser
{
    public IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        if (string.IsNullOrWhiteSpace(jwt))
            return [];

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwt);
            return token.Claims;
        }
        catch
        {
            return [];
        }
    }

    public bool IsTokenExpired(string jwt)
    {
        var expiration = GetTokenExpiration(jwt);
        if (expiration == null)
            return true;

        return expiration.Value <= DateTime.UtcNow;
    }

    public DateTime? GetTokenExpiration(string jwt)
    {
        if (string.IsNullOrWhiteSpace(jwt))
            return null;

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwt);
            return token.ValidTo;
        }
        catch
        {
            return null;
        }
    }
}
