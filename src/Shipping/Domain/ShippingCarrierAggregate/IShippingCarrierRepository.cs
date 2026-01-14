namespace Shipping.Domain;

public interface IShippingCarrierRepository : IRepository<ShippingCarrier>
{
    Task<ShippingCarrier?> GetByProviderAsync(ShippingProvider provider, CancellationToken ct = default);
    Task<ShippingCarrier?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<List<ShippingCarrier>> GetEnabledCarriersAsync(CancellationToken ct = default);
    Task<List<ShippingCarrier>> GetByRegionAsync(string region, CancellationToken ct = default);
}
