using Kernel.Application.Interfaces;
using Shipping.Domain;

namespace Shipping.Application;

public interface IShippingUnitOfWork : IUnitOfWork
{
    IShipmentRepository Shipments { get; }
    IShippingCarrierRepository ShippingCarriers { get; }
}
