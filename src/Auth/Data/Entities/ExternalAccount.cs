namespace Auth;

/// <summary>
/// External login information (Google, Facebook, etc.)
/// </summary>
public class ExternalAccount : BaseEntity
{
    public Guid AccountId { get;  set; }
    public Account? Account { get;  set; }

    public string Provider { get;  set; } = string.Empty;      // "Google", "Facebook"
    public string ProviderKey { get;  set; } = string.Empty;   // External user ID
    public DateTimeOffset LinkedAt { get;  set; }
}
