namespace Auth;

/// <summary>
/// Persisted refresh token for secure rotation and revocation.
/// The raw token is never stored, only the SHA-256 hash.
/// </summary>
public class RefreshToken : BaseEntity
{
    public Guid AccountId { get; set; }
    public Account? Account { get; set; }

    public string TokenHash { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAtUtc { get; set; }

    public DateTimeOffset? RevokedAtUtc { get; set; }
    public string? ReplacedByTokenHash { get; set; }

    public string? CreatedByIp { get; set; }
    public string? RevokedByIp { get; set; }
}
