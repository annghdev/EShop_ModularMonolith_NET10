using Inventory.Domain;

namespace Inventory.Infrastructure;

public class InventorySeeder(InventoryDbContext context)
{
    public async Task SeedAsync()
    {
        if (!await context.Warehouses.AnyAsync())
        {
            var defaultWarehouse = Warehouse.Create(
                code: "DEFAULT",
                name: "Default Warehouse",
                address: null,
                isDefault: true);

            context.Warehouses.Add(defaultWarehouse);
            await context.SaveChangesAsync();
        }
    }
}
