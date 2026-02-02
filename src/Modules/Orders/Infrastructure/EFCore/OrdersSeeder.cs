using Orders.Domain;

namespace Orders.Infrastructure;

public class OrdersSeeder
{
    private readonly OrdersDbContext _context;

    public OrdersSeeder(OrdersDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        if (_context.Orders.Any()) return;

        var order1 = Order.Create(
            Guid.CreateVersion7(),
            new Address(
                "Nguyen Van A",
                "+84901234567",
                "12 Nguyen Trai",
                "Phuong 1",
                "Quan 1",
                "Ho Chi Minh",
                "Vietnam",
                "700000"),
            PaymentMethod.COD,
            ShippingMethod.Standard,
            "Giao buoi sang");
        order1.Id = Guid.CreateVersion7();
        order1.AddItem(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "SKU-ORD-001",
            "Basic T-Shirt",
            "Red / M",
            "https://example.com/products/tshirt-red-m.png",
            new Money(150_000),
            quantity: 2);
        order1.AddItem(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "SKU-ORD-002",
            "Denim Jeans",
            "Blue / 32",
            "https://example.com/products/jeans-blue-32.png",
            new Money(420_000),
            quantity: 1);

        var order2 = Order.Create(
            Guid.CreateVersion7(),
            new Address(
                "Tran Thi B",
                "+84987654321",
                "88 Le Loi",
                "Phuong Ben Thanh",
                "Quan 1",
                "Ho Chi Minh",
                "Vietnam",
                "700000"),
            PaymentMethod.Online,
            ShippingMethod.Fast,
            "Goi truoc khi giao");
        order2.Id = Guid.CreateVersion7();
        order2.AddItem(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "SKU-ORD-003",
            "Wireless Mouse",
            "Black",
            "https://example.com/products/mouse-black.png",
            new Money(199_000),
            quantity: 1);

        await _context.Orders.AddRangeAsync(order1, order2);
        await _context.SaveChangesAsync();
    }
}