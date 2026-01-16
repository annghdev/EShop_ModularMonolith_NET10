using Auth.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Auth.Data;

/// <summary>
/// Seeder for Auth module - creates default roles, admin account, and permissions
/// </summary>
public class AuthSeeder
{
    private readonly UserManager<Account> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly ILogger<AuthSeeder> _logger;

    // Default admin credentials - should be changed after first login
    private const string AdminEmail = "admin@eshop.com";
    private const string AdminPassword = "123123";
    private const string AdminUserName = "admin";

    public AuthSeeder(
        UserManager<Account> userManager,
        RoleManager<Role> roleManager,
        ILogger<AuthSeeder> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        await SeedRolesAsync();
        await SeedAdminAccountAsync();
        await SeedAdminPermissionsAsync();
        
        _logger.LogInformation("Auth seed completed successfully");
    }

    private async Task SeedRolesAsync()
    {
        var roles = new[]
        {
            new Role
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = DefaultRoles.Admin,
                NormalizedName = DefaultRoles.Admin.ToUpperInvariant(),
                DisplayName = "Administrator",
                Description = "Full system access",
                Icon = "👑",
                IsSystemRole = true
            },
            new Role
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = DefaultRoles.Customer,
                NormalizedName = DefaultRoles.Customer.ToUpperInvariant(),
                DisplayName = "Customer",
                Description = "Regular customer access",
                Icon = "👤",
                IsSystemRole = true
            },
            new Role
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Name = DefaultRoles.Staff,
                NormalizedName = DefaultRoles.Staff.ToUpperInvariant(),
                DisplayName = "Staff",
                Description = "Staff member access",
                Icon = "👨‍💼",
                IsSystemRole = true
            }
        };

        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role.Name!))
            {
                var result = await _roleManager.CreateAsync(role);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Created role: {RoleName}", role.Name);
                }
                else
                {
                    _logger.LogError("Failed to create role {RoleName}: {Errors}", 
                        role.Name, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
    }

    private async Task SeedAdminAccountAsync()
    {
        var adminUser = await _userManager.FindByEmailAsync(AdminEmail);
        if (adminUser != null)
        {
            _logger.LogInformation("Admin account already exists");
            return;
        }

        adminUser = new Account
        {
            Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            UserName = AdminUserName,
            Email = AdminEmail,
            EmailConfirmed = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var result = await _userManager.CreateAsync(adminUser, AdminPassword);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(adminUser, DefaultRoles.Admin);
            _logger.LogInformation("Created admin account: {Email}", AdminEmail);
        }
        else
        {
            _logger.LogError("Failed to create admin account: {Errors}",
                string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }

    private async Task SeedAdminPermissionsAsync()
    {
        var adminRole = await _roleManager.FindByNameAsync(DefaultRoles.Admin);
        if (adminRole == null) return;

        var existingClaims = await _roleManager.GetClaimsAsync(adminRole);
        if (existingClaims.Any(c => c.Type == Permissions.ClaimType))
        {
            _logger.LogInformation("Admin permissions already seeded");
            return;
        }

        // Grant all permissions to Admin role
        var allPermissions = GetAllPermissions();
        foreach (var permission in allPermissions)
        {
            var claim = new System.Security.Claims.Claim(Permissions.ClaimType, permission);
            await _roleManager.AddClaimAsync(adminRole, claim);
        }

        _logger.LogInformation("Seeded {Count} permissions for Admin role", allPermissions.Count);
    }

    private static List<string> GetAllPermissions()
    {
        var permissions = new List<string>();

        // Auth module permissions
        permissions.AddRange(Permissions.CrudFor(PermissionModules.Auth, PermissionResources.Auth.Role));
        permissions.AddRange(Permissions.CrudFor(PermissionModules.Auth, PermissionResources.Auth.Permission));

        // Users module permissions
        permissions.AddRange(Permissions.CrudFor(PermissionModules.Users, PermissionResources.Users.User));
        permissions.AddRange(Permissions.CrudFor(PermissionModules.Users, PermissionResources.Users.Profile));

        // Catalog module permissions
        permissions.AddRange(Permissions.CrudFor(PermissionModules.Catalog, PermissionResources.Catalog.Product));
        permissions.AddRange(Permissions.CrudFor(PermissionModules.Catalog, PermissionResources.Catalog.Category));
        permissions.AddRange(Permissions.CrudFor(PermissionModules.Catalog, PermissionResources.Catalog.Brand));
        permissions.AddRange(Permissions.CrudFor(PermissionModules.Catalog, PermissionResources.Catalog.Collection));
        permissions.AddRange(Permissions.CrudFor(PermissionModules.Catalog, PermissionResources.Catalog.Attribute));

        // Inventory module permissions
        permissions.AddRange(Permissions.CrudFor(PermissionModules.Inventory, PermissionResources.Inventory.Stock));
        permissions.AddRange(Permissions.CrudFor(PermissionModules.Inventory, PermissionResources.Inventory.Warehouse));
        permissions.AddRange(Permissions.CrudFor(PermissionModules.Inventory, PermissionResources.Inventory.StockMovement));

        // Pricing module permissions
        permissions.AddRange(Permissions.CrudFor(PermissionModules.Pricing, PermissionResources.Pricing.Price));
        permissions.AddRange(Permissions.CrudFor(PermissionModules.Pricing, PermissionResources.Pricing.Coupon));
        permissions.AddRange(Permissions.CrudFor(PermissionModules.Pricing, PermissionResources.Pricing.Promotion));

        // Orders module permissions
        permissions.AddRange(Permissions.CrudFor(PermissionModules.Orders, PermissionResources.Orders.Order));
        permissions.Add(Permissions.For(PermissionModules.Orders, PermissionResources.Orders.Order, PermissionActions.Approve));
        permissions.AddRange(Permissions.CrudFor(PermissionModules.Orders, PermissionResources.Orders.OrderItem));

        // Payment module permissions
        permissions.AddRange(Permissions.CrudFor(PermissionModules.Payment, PermissionResources.Payment.Method));
        permissions.AddRange(Permissions.CrudFor(PermissionModules.Payment, PermissionResources.Payment.Transaction));
        permissions.AddRange(Permissions.CrudFor(PermissionModules.Payment, PermissionResources.Payment.Gateway));

        // Shipping module permissions
        permissions.AddRange(Permissions.CrudFor(PermissionModules.Shipping, PermissionResources.Shipping.Shipment));
        permissions.AddRange(Permissions.CrudFor(PermissionModules.Shipping, PermissionResources.Shipping.Carrier));

        // Report module permissions - Read + Export only
        permissions.Add(Permissions.For(PermissionModules.Report, PermissionResources.Report.SalesReport, PermissionActions.Read));
        permissions.Add(Permissions.For(PermissionModules.Report, PermissionResources.Report.SalesReport, PermissionActions.Export));
        permissions.Add(Permissions.For(PermissionModules.Report, PermissionResources.Report.InventoryReport, PermissionActions.Read));
        permissions.Add(Permissions.For(PermissionModules.Report, PermissionResources.Report.InventoryReport, PermissionActions.Export));
        permissions.Add(Permissions.For(PermissionModules.Report, PermissionResources.Report.Dashboard, PermissionActions.Read));

        return permissions;
    }
}
