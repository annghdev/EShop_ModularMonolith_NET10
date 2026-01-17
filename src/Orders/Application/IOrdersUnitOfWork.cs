using Kernel.Application.Interfaces;
using Orders.Domain;

namespace Orders.Application;

public interface IOrdersUnitOfWork : IUnitOfWork
{
    IOrderRepository Orders { get; }
}
