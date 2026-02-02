using Payment.Domain;
using System.Reflection;

namespace Payment.Infrastructure;

public class PaymentDbContext(DbContextOptions<PaymentDbContext> options) : BaseDbContext(options)
{
    public DbSet<PaymentGateway> PaymentGateways { get; set; }
    public DbSet<PaymentGatewaySetting> PaymentGatewaySettings { get; set; }
    public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
    public DbSet<PaymentRefund> PaymentRefunds { get; set; }

    protected override Assembly GetAssembly()
    {
        return Assembly.GetExecutingAssembly();
    }
}
