using ShoppingCart.Domain;

namespace ShoppingCart.Infrastructure;

public class ShoppingCartSeeder
{
    private readonly ShoppingCartDbContext _context;

    public ShoppingCartSeeder(ShoppingCartDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        if (_context.Carts.Any()) return;

        var registeredCart = Cart.CreateForCustomer(Guid.CreateVersion7());
        registeredCart.Id = Guid.CreateVersion7();
        registeredCart.AddItem(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "SKU-REG-001",
            "Basic T-Shirt",
            "Red / M",
            "https://example.com/products/tshirt-red-m.png",
            new Money(150_000),
            quantity: 2);
        registeredCart.AddItem(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "SKU-REG-002",
            "Denim Jeans",
            "Blue / 32",
            "https://example.com/products/jeans-blue-32.png",
            new Money(420_000),
            quantity: 1);

        var guestCart = Cart.CreateForGuest("guest-001");
        guestCart.Id = Guid.CreateVersion7();
        guestCart.AddItem(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "SKU-GST-001",
            "Wireless Mouse",
            "Black",
            "https://example.com/products/mouse-black.png",
            new Money(199_000),
            quantity: 1);

        await _context.Carts.AddRangeAsync(registeredCart, guestCart);
        await _context.SaveChangesAsync();
    }
}
