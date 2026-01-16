namespace Auth.Constants;

/// <summary>
/// Module names for permission organization
/// </summary>
public static class PermissionModules
{
    public const string Auth = "Auth";
    public const string Users = "Users";
    public const string Catalog = "Catalog";
    public const string Inventory = "Inventory";
    public const string Pricing = "Pricing";
    public const string Orders = "Orders";
    public const string Payment = "Payment";
    public const string Shipping = "Shipping";
    public const string ShoppingCart = "ShoppingCart";
    public const string Report = "Report";

    public static readonly string[] All = [Auth, Users, Catalog, Inventory, Pricing, Orders, Payment, Shipping, ShoppingCart, Report];
}
