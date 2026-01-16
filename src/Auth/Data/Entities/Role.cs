using Microsoft.AspNetCore.Identity;

namespace Auth;

public class Role : IdentityRole<Guid>
{
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public bool IsSystemRole { get; set; }  // Admin, Customer - cannot be deleted
}
