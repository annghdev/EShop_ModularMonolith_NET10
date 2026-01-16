namespace Auth.Constants;

/// <summary>
/// Available permission actions in the system
/// </summary>
public static class PermissionActions
{
    public const string Create = "Create";
    public const string Read = "Read";
    public const string Update = "Update";
    public const string Delete = "Delete";
    public const string Export = "Export";
    public const string Approve = "Approve";

    public static readonly string[] All = [Create, Read, Update, Delete, Export, Approve];
}
