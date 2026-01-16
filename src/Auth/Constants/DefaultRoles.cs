namespace Auth.Constants;

/// <summary>
/// Default role names in the system
/// </summary>
public static class DefaultRoles
{
    public const string Admin = "Admin";
    public const string Customer = "Customer";
    public const string Staff = "Staff";

    public static readonly string[] SystemRoles = [Admin, Customer, Staff];
}
