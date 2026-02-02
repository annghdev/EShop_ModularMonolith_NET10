using System.Reflection;
using Orders.Domain;

namespace Orders.Infrastructure;

public class OrdersDbContext(DbContextOptions<OrdersDbContext> options)
    : BaseDbContext(options)
{
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<OrderDiscount> OrderDiscounts { get; set; }
    public DbSet<OrderStatusHistory> OrderStatusHistory { get; set; }

    protected override Assembly GetAssembly()
    {
        return Assembly.GetExecutingAssembly();
    }
}
