using Users.Application;
using Users.Domain;
using Users.Infrastructure.EFCore.Repositories;

namespace Users.Infrastructure;

public class UsersUnitOfWork(UsersDbContext context, ICurrentUser user, IPublisher publisher)
    : BaseUnitOfWork<UsersDbContext>(context, user, publisher), IUsersUnitOfWork
{
    private ICustomerRepository? _customers;
    private IEmployeeRepository? _employees;
    private IGuestRepository? _guests;

    public ICustomerRepository Customers => _customers ??= new CustomerRepository(context);
    public IEmployeeRepository Employees => _employees ??= new EmployeeRepository(context);
    public IGuestRepository Guests => _guests ??= new GuestRepository(context);
}
