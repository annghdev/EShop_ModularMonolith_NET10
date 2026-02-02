namespace Shipping.Domain;

public interface IShipmentRepository : IRepository<Shipment>
{
    Task<Shipment?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default);
    Task<Shipment?> GetByTrackingNumberAsync(string trackingNumber, CancellationToken ct = default);
    Task<Shipment?> GetByShipmentNumberAsync(string shipmentNumber, CancellationToken ct = default);
    Task<List<Shipment>> GetByStatusAsync(ShipmentStatus status, CancellationToken ct = default);
    Task<List<Shipment>> GetByCarrierIdAsync(Guid carrierId, CancellationToken ct = default);
    Task<List<Shipment>> GetPendingDeliveryAsync(CancellationToken ct = default);
}
