using Kernel.Application.Interfaces;
using Users.Domain;

namespace Users.Application;

public interface IUsersUnitOfWork : IUnitOfWork
{
    ICustomerRepository Customers { get; }
    IEmployeeRepository Employees { get; }
    IGuestRepository Guests { get; }
}
