namespace Auth.Constants;

/// <summary>
/// Helper class for building and parsing permission claim values
/// </summary>
public static class Permissions
{
    public const string ClaimType = "permission";
    public const char Separator = ':';

    /// <summary>
    /// Builds a permission claim value in format "Module:Resource:Action"
    /// </summary>
    public static string For(string module, string resource, string action)
        => $"{module}{Separator}{resource}{Separator}{action}";

    /// <summary>
    /// Parses a permission claim value into its components
    /// </summary>
    public static (string Module, string Resource, string Action)? Parse(string permission)
    {
        var parts = permission.Split(Separator);
        if (parts.Length != 3) return null;
        return (parts[0], parts[1], parts[2]);
    }

    /// <summary>
    /// Checks if the permission matches the specified module, resource, and action
    /// </summary>
    public static bool Matches(string permission, string module, string resource, string action)
        => permission == For(module, resource, action);

    /// <summary>
    /// Gets all CRUD permissions for a resource
    /// </summary>
    public static string[] CrudFor(string module, string resource) =>
    [
        For(module, resource, PermissionActions.Create),
        For(module, resource, PermissionActions.Read),
        For(module, resource, PermissionActions.Update),
        For(module, resource, PermissionActions.Delete),
        For(module, resource, PermissionActions.Approve),
        For(module, resource, PermissionActions.Export),
    ];

    /// <summary>
    /// Gets all permissions for a resource including all actions
    /// </summary>
    public static string[] AllFor(string module, string resource) =>
        PermissionActions.All.Select(action => For(module, resource, action)).ToArray();
}
