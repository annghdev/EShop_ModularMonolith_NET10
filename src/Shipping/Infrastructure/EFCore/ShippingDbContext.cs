using Shipping.Domain;
using System.Reflection;

namespace Shipping.Infrastructure;

public class ShippingDbContext(DbContextOptions<ShippingDbContext> options) : BaseDbContext(options)
{
    public DbSet<Shipment> Shipments { get; set; }
    public DbSet<ShipmentItem> ShipmentItems { get; set; }
    public DbSet<TrackingEvent> TrackingEvents { get; set; }
    public DbSet<ShippingCarrier> ShippingCarriers { get; set; }
    public DbSet<ShippingCarrierSetting> ShippingCarrierSettings { get; set; }

    protected override Assembly GetAssembly()
    {
        return Assembly.GetExecutingAssembly();
    }
}
