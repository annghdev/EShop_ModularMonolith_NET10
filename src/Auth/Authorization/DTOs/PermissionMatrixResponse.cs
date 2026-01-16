namespace Auth.Authorization.DTOs;

/// <summary>
/// Permission matrix response for role or user permissions
/// Structured as: Modules -> Resources -> Actions (bool)
/// </summary>
public record PermissionMatrixResponse
{
    public List<ModulePermissions> Modules { get; init; } = [];
}

public record ModulePermissions
{
    public string ModuleName { get; init; } = string.Empty;
    public List<ResourcePermissions> Resources { get; init; } = [];
}

public record ResourcePermissions
{
    public string ResourceName { get; init; } = string.Empty;
    /// <summary>
    /// Dictionary of action name to permission state
    /// Example: { "Create": true, "Read": true, "Update": false, "Delete": false }
    /// </summary>
    public Dictionary<string, bool> Actions { get; init; } = new();
}
