using Shipping.Application;
using Shipping.Domain;
using Shipping.Infrastructure.EFCore.Repositories;

namespace Shipping.Infrastructure;

public sealed class ShippingUnitOfWork(ShippingDbContext context, IUserContext user, IPublisher publisher)
    : BaseUnitOfWork<ShippingDbContext>(context, user, publisher), IShippingUnitOfWork
{
    private IShipmentRepository? _shipmentRepository;
    private IShippingCarrierRepository? _shippingCarrierRepository;

    public IShipmentRepository Shipments =>
        _shipmentRepository ??= new ShipmentRepository(context);

    public IShippingCarrierRepository ShippingCarriers =>
        _shippingCarrierRepository ??= new ShippingCarrierRepository(context);
}
