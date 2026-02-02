using Users.Domain;

namespace Users.Infrastructure;

public class UsersSeeder
{
    private readonly UsersDbContext _context;

    public UsersSeeder(UsersDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        await SeedCustomersAsync();
        await SeedEmployeesAsync();
        await SeedGuestsAsync();
        await _context.SaveChangesAsync();
    }

    private async Task SeedCustomersAsync()
    {
        if (_context.Customers.Any()) return;

        var customer1 = Customer.Create("Nguyen Van A", new Email("customer1@example.com"));
        customer1.Id = Guid.CreateVersion7();
        customer1.UpdateProfile("Nguyen Van A", new PhoneNumber("+84901234567"), DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-28)), Gender.Male);
        customer1.AddAddress(new Address(
            "Nguyen Van A",
            "+84901234567",
            "12 Nguyen Trai",
            "Phuong 1",
            "Quan 1",
            "Ho Chi Minh",
            "Vietnam",
            "700000"), AddressType.Home, isDefault: true, label: "Nha");

        var customer2 = Customer.Create("Tran Thi B", new Email("customer2@example.com"));
        customer2.Id = Guid.CreateVersion7();
        customer2.UpdateProfile("Tran Thi B", new PhoneNumber("+84987654321"), DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-24)), Gender.Female);
        customer2.AddAddress(new Address(
            "Tran Thi B",
            "+84987654321",
            "88 Le Loi",
            "Phuong Ben Thanh",
            "Quan 1",
            "Ho Chi Minh",
            "Vietnam",
            "700000"), AddressType.Work, isDefault: true, label: "Cong ty");

        await _context.Customers.AddRangeAsync(customer1, customer2);
    }

    private async Task SeedEmployeesAsync()
    {
        if (_context.Employees.Any()) return;

        var employee1 = Employee.Create(
            "EMP-001",
            "Le Hoang",
            new Email("employee1@example.com"),
            Guid.CreateVersion7());
        employee1.Id = Guid.CreateVersion7();

        var employee2 = Employee.Create(
            "EMP-002",
            "Pham Minh",
            new Email("employee2@example.com"),
            Guid.CreateVersion7());
        employee2.Id = Guid.CreateVersion7();

        await _context.Employees.AddRangeAsync(employee1, employee2);
    }

    private async Task SeedGuestsAsync()
    {
        if (_context.Guests.Any()) return;

        var guest1 = Guest.Create("guest-001");
        guest1.Id = Guid.CreateVersion7();
        guest1.UpdateInfo("guest1@example.com", "+84900000001", "Guest One");
        guest1.SetLastShippingAddress(new Address(
            "Guest One",
            "+84900000001",
            "1 Tran Hung Dao",
            "Phuong 5",
            "Quan 5",
            "Ho Chi Minh",
            "Vietnam",
            "700000"));

        var guest2 = Guest.Create("guest-002");
        guest2.Id = Guid.CreateVersion7();
        guest2.UpdateInfo("guest2@example.com", "+84900000002", "Guest Two");

        await _context.Guests.AddRangeAsync(guest1, guest2);
    }
}
