namespace Auth.Constants;

/// <summary>
/// Resource names organized by module for permission management
/// </summary>
public static class PermissionResources
{
    // Auth module resources
    public static class Auth
    {
        public const string Role = "Role";
        public const string Permission = "Permission";

        public static readonly string[] All = [Role, Permission];
    }

    // Users module resources
    public static class Users
    {
        public const string User = "User";
        public const string Profile = "Profile";

        public static readonly string[] All = [User, Profile];
    }

    // Catalog module resources
    public static class Catalog
    {
        public const string Product = "Product";
        public const string Category = "Category";
        public const string Brand = "Brand";
        public const string Collection = "Collection";
        public const string Attribute = "Attribute";

        public static readonly string[] All = [Product, Category, Brand, Collection, Attribute];
    }

    // Inventory module resources
    public static class Inventory
    {
        public const string Stock = "Stock";
        public const string Warehouse = "Warehouse";
        public const string StockMovement = "StockMovement";

        public static readonly string[] All = [Stock, Warehouse, StockMovement];
    }

    // Pricing module resources
    public static class Pricing
    {
        public const string Price = "Price";
        public const string Coupon = "Coupon";
        public const string Promotion = "Promotion";

        public static readonly string[] All = [Price, Coupon, Promotion];
    }

    // Orders module resources
    public static class Orders
    {
        public const string Order = "Order";
        public const string OrderItem = "OrderItem";

        public static readonly string[] All = [Order, OrderItem];
    }

    // Payment module resources
    public static class Payment
    {
        public const string Method = "Method";
        public const string Transaction = "Transaction";
        public const string Gateway = "Gateway";

        public static readonly string[] All = [Method, Transaction, Gateway];
    }

    // Shipping module resources
    public static class Shipping
    {
        public const string Shipment = "Shipment";
        public const string Carrier = "Carrier";

        public static readonly string[] All = [Shipment, Carrier];
    }

    // ShoppingCart module resources
    public static class ShoppingCart
    {
        public const string Cart = "Cart";
        public const string CartItem = "CartItem";

        public static readonly string[] All = [Cart, CartItem];
    }

    // Report module resources
    public static class Report
    {
        public const string SalesReport = "SalesReport";
        public const string InventoryReport = "InventoryReport";
        public const string Dashboard = "Dashboard";

        public static readonly string[] All = [SalesReport, InventoryReport, Dashboard];
    }
}
