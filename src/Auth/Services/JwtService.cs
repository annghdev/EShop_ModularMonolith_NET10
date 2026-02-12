using Contracts;
using Auth.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Auth.Services;

/// <summary>
/// JWT token generation and validation service
/// </summary>
public class JwtService : IJwtService
{
    private readonly UserManager<Account> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly AuthDbContext _dbContext;
    private readonly JwtSettings _jwtSettings;

    public JwtService(
        UserManager<Account> userManager,
        RoleManager<Role> roleManager,
        AuthDbContext dbContext,
        IOptions<JwtSettings> jwtSettings)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _dbContext = dbContext;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<TokensResult> GenerateTokensAsync(Account account, string? createdByIp = null)
    {
        var claims = await GetClaimsAsync(account);
        var accessToken = GenerateAccessToken(claims);
        var refreshToken = GenerateRefreshToken();
        var refreshTokenHash = HashToken(refreshToken);

        _dbContext.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.CreateVersion7(),
            AccountId = account.Id,
            TokenHash = refreshTokenHash,
            ExpiresAtUtc = DateTimeOffset.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedByIp = createdByIp
        });

        await _dbContext.SaveChangesAsync();

        return new TokensResult(accessToken, refreshToken);
    }

    public async Task<TokensResult> RefreshTokensAsync(string refreshToken, string? createdByIp = null)
    {
        var tokenHash = HashToken(refreshToken);
        var storedToken = await _dbContext.RefreshTokens
            .Include(x => x.Account)
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash);

        if (storedToken?.Account is null)
        {
            throw new UnauthorizedAccessException("Invalid refresh token.");
        }

        if (storedToken.RevokedAtUtc.HasValue)
        {
            throw new UnauthorizedAccessException("Refresh token has been revoked.");
        }

        if (storedToken.ExpiresAtUtc <= DateTimeOffset.UtcNow)
        {
            throw new UnauthorizedAccessException("Refresh token has expired.");
        }

        var claims = await GetClaimsAsync(storedToken.Account);
        var accessToken = GenerateAccessToken(claims);
        var newRefreshToken = GenerateRefreshToken();
        var newRefreshTokenHash = HashToken(newRefreshToken);

        storedToken.RevokedAtUtc = DateTimeOffset.UtcNow;
        storedToken.RevokedByIp = createdByIp;
        storedToken.ReplacedByTokenHash = newRefreshTokenHash;

        _dbContext.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.CreateVersion7(),
            AccountId = storedToken.AccountId,
            TokenHash = newRefreshTokenHash,
            ExpiresAtUtc = DateTimeOffset.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedByIp = createdByIp
        });

        await _dbContext.SaveChangesAsync();

        return new TokensResult(accessToken, newRefreshToken);
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken, string? revokedByIp = null)
    {
        var tokenHash = HashToken(refreshToken);
        var storedToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash);

        if (storedToken is null || storedToken.RevokedAtUtc.HasValue)
        {
            return;
        }

        storedToken.RevokedAtUtc = DateTimeOffset.UtcNow;
        storedToken.RevokedByIp = revokedByIp;
        await _dbContext.SaveChangesAsync();
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

        try
        {
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);

            return principal;
        }
        catch
        {
            return null;
        }
    }

    private async Task<List<Claim>> GetClaimsAsync(Account account)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, account.Id.ToString()),
            new(ClaimTypes.Email, account.Email ?? string.Empty),
            new(ClaimTypes.Name, account.UserName ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Add roles
        var roles = await _userManager.GetRolesAsync(account);
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));

            // Add role claims (permissions)
            var roleEntity = await _roleManager.FindByNameAsync(role);
            if (roleEntity != null)
            {
                var roleClaims = await _roleManager.GetClaimsAsync(roleEntity);
                claims.AddRange(roleClaims);
            }
        }

        // Add user-specific claims (override permissions)
        var userClaims = await _userManager.GetClaimsAsync(account);
        claims.AddRange(userClaims);

        return claims;
    }

    private string GenerateAccessToken(IEnumerable<Claim> claims)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTimeOffset.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes).UtcDateTime,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }
}
