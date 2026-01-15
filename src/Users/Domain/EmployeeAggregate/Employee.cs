namespace Users.Domain;

/// <summary>
/// Employee aggregate - represents internal staff/admin
/// </summary>
public class Employee : AggregateRoot
{
    public string EmployeeCode { get; private set; } = string.Empty;  // VD: "EMP-001"
    public string FullName { get; private set; } = string.Empty;
    public Email Email { get; private set; }
    public PhoneNumber? Phone { get; private set; }
    public string? Department { get; private set; }
    public string? Position { get; private set; }
    public string? AvatarUrl { get; private set; }

    // Account link
    public Guid AccountId { get; private set; }

    // Employment info
    public DateOnly? HireDate { get; private set; }
    public EmployeeStatus Status { get; private set; }

    private Employee() { } // EF Core

    #region Factory Methods

    public static Employee Create(string employeeCode, string fullName, Email email, Guid accountId)
    {
        if (string.IsNullOrWhiteSpace(employeeCode))
            throw new DomainException("Employee code is required");

        if (string.IsNullOrWhiteSpace(fullName))
            throw new DomainException("Full name is required");

        if (email == null)
            throw new DomainException("Email is required");

        if (accountId == Guid.Empty)
            throw new DomainException("Account ID is required");

        return new Employee
        {
            EmployeeCode = employeeCode.ToUpperInvariant(),
            FullName = fullName.Trim(),
            Email = email,
            AccountId = accountId,
            Status = EmployeeStatus.Pending,
            HireDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };
    }

    #endregion

    #region Methods

    public void UpdateInfo(string fullName, PhoneNumber? phone, string? department, string? position)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new DomainException("Full name is required");

        FullName = fullName.Trim();
        Phone = phone;
        Department = department?.Trim();
        Position = position?.Trim();

        IncreaseVersion();
    }

    public void UpdateAvatar(string? avatarUrl)
    {
        AvatarUrl = avatarUrl;
        IncreaseVersion();
    }

    public void Activate()
    {
        if (Status == EmployeeStatus.Terminated)
            throw new DomainException("Cannot activate terminated employee");

        Status = EmployeeStatus.Active;
        IncreaseVersion();
    }

    public void Deactivate()
    {
        if (Status == EmployeeStatus.Terminated)
            throw new DomainException("Cannot deactivate terminated employee");

        Status = EmployeeStatus.Inactive;
        IncreaseVersion();
    }

    public void Terminate()
    {
        Status = EmployeeStatus.Terminated;
        IncreaseVersion();
    }

    #endregion
}
