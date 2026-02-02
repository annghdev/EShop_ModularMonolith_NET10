using Shipping.Domain;

namespace Shipping.Infrastructure.EFCore.Repositories;

public class ShipmentRepository(ShippingDbContext db)
    : BaseRepository<Shipment, ShippingDbContext>(db), IShipmentRepository
{
    public override async Task<Shipment> LoadFullAggregate(Guid id, bool changeTracking = true)
    {
        var query = changeTracking ? dbSet : dbSet.AsNoTracking();

        return await query
            .Include(s => s.Items)
            .Include(s => s.TrackingEvents)
            .Include(s => s.Carrier)
            .FirstOrDefaultAsync(s => s.Id == id)
            ?? throw new NotFoundException("Shipment", id);
    }

    public override async Task<IEnumerable<Shipment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbSet
            .AsNoTracking()
            .Include(s => s.Items)
            .Include(s => s.TrackingEvents)
            .Include(s => s.Carrier)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Shipment?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default)
    {
        return await dbSet
            .AsNoTracking()
            .Include(s => s.Items)
            .Include(s => s.TrackingEvents)
            .Include(s => s.Carrier)
            .FirstOrDefaultAsync(s => s.OrderId == orderId, ct);
    }

    public async Task<Shipment?> GetByTrackingNumberAsync(string trackingNumber, CancellationToken ct = default)
    {
        return await dbSet
            .AsNoTracking()
            .Include(s => s.Items)
            .Include(s => s.TrackingEvents)
            .Include(s => s.Carrier)
            .FirstOrDefaultAsync(s => s.TrackingNumber == trackingNumber, ct);
    }

    public async Task<Shipment?> GetByShipmentNumberAsync(string shipmentNumber, CancellationToken ct = default)
    {
        return await dbSet
            .AsNoTracking()
            .Include(s => s.Items)
            .Include(s => s.TrackingEvents)
            .Include(s => s.Carrier)
            .FirstOrDefaultAsync(s => s.ShipmentNumber == shipmentNumber, ct);
    }

    public async Task<List<Shipment>> GetByStatusAsync(ShipmentStatus status, CancellationToken ct = default)
    {
        return await dbSet
            .AsNoTracking()
            .Include(s => s.Items)
            .Include(s => s.TrackingEvents)
            .Include(s => s.Carrier)
            .Where(s => s.Status == status)
            .ToListAsync(ct);
    }

    public async Task<List<Shipment>> GetByCarrierIdAsync(Guid carrierId, CancellationToken ct = default)
    {
        return await dbSet
            .AsNoTracking()
            .Include(s => s.Items)
            .Include(s => s.TrackingEvents)
            .Include(s => s.Carrier)
            .Where(s => s.CarrierId == carrierId)
            .ToListAsync(ct);
    }

    public async Task<List<Shipment>> GetPendingDeliveryAsync(CancellationToken ct = default)
    {
        return await dbSet
            .AsNoTracking()
            .Include(s => s.Items)
            .Include(s => s.TrackingEvents)
            .Include(s => s.Carrier)
            .Where(s => s.Status != ShipmentStatus.Delivered
                        && s.Status != ShipmentStatus.Returned
                        && s.Status != ShipmentStatus.Cancelled)
            .ToListAsync(ct);
    }
}
