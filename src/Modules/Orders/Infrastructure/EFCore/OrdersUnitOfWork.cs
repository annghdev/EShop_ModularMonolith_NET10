using Orders.Application;
using Orders.Domain;
using Orders.Infrastructure.EFCore.Repositories;

namespace Orders.Infrastructure;

public class OrdersUnitOfWork(OrdersDbContext context, IUserContext user, IPublisher publisher)
    : BaseUnitOfWork<OrdersDbContext>(context, user, publisher), IOrdersUnitOfWork
{
    private IOrderRepository? _orders;

    public IOrderRepository Orders => _orders ??= new OrderRepository(context);
}
