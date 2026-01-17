using ShoppingCart.Application;
using ShoppingCart.Domain;
using ShoppingCart.Infrastructure.EFCore.Repositories;

namespace ShoppingCart.Infrastructure;

public class ShoppingCartUnitOfWork(ShoppingCartDbContext context, ICurrentUser user, IPublisher publisher)
    : BaseUnitOfWork<ShoppingCartDbContext>(context, user, publisher), IShoppingCartUnitOfWork
{
    private ICartRepository? _carts;

    public ICartRepository Carts => _carts ??= new CartRepository(context);
}
