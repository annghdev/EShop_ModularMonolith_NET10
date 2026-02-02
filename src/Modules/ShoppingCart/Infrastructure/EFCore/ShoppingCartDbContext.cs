using System.Reflection;
using ShoppingCart.Domain;

namespace ShoppingCart.Infrastructure;

public class ShoppingCartDbContext(DbContextOptions<ShoppingCartDbContext> options)
    : BaseDbContext(options)
{
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }

    protected override Assembly GetAssembly()
    {
        return Assembly.GetExecutingAssembly();
    }
}
