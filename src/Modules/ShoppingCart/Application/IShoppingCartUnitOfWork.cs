using Kernel.Application.Interfaces;
using ShoppingCart.Domain;

namespace ShoppingCart.Application;

public interface IShoppingCartUnitOfWork : IUnitOfWork
{
    ICartRepository Carts { get; }
}
