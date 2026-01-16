using Microsoft.AspNetCore.Identity;

namespace Auth;

public class Account : IdentityUser<Guid>
{
    public string? AvatarUrl { get; set; }

    // External logins
    public ICollection<ExternalAccount>? ExternalAccounts { get; set; }

    // Audit fields
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
