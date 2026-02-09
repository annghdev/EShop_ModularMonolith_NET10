using Inventory.Domain;

namespace Inventory.Infrastructure;

public class InventorySeeder(InventoryDbContext context)
{
    public async Task SeedAsync()
    {
        if (!await context.Warehouses.AnyAsync())
        {
            var defaultWarehouse = Warehouse.Create(
                code: "WH-HCM",
                name: "Kho Hồ Chí Minh",
                address: new Address("Kho HCM", "02838234567", "123 Nguyễn Huệ", "Phường Bến Nghé", "Quận 1", "TP.HCM", "Vietnam", "700000"),
                isDefault: true);

            var hnWarehouse = Warehouse.Create(
                code: "WH-HN",
                name: "Kho Hà Nội",
                address: new Address("Kho HN", "02438234567", "456 Phố Huế", "Phường Phố Huế", "Quận Hai Bà Trưng", "Hà Nội", "Vietnam", "100000"),
                isDefault: false);

            context.Warehouses.AddRange(defaultWarehouse, hnWarehouse);
            await context.SaveChangesAsync();
        }
    }
}
