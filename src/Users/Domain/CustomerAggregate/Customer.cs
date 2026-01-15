namespace Users.Domain;

/// <summary>
/// Customer aggregate - represents a registered customer profile
/// </summary>
public class Customer : AggregateRoot
{
    public string FullName { get; private set; } = string.Empty;
    public Email Email { get; private set; }
    public PhoneNumber? Phone { get; private set; }
    public DateOnly? DateOfBirth { get; private set; }
    public Gender? Gender { get; private set; }
    public string? AvatarUrl { get; private set; }
    public Guid? AccountId { get; private set; }

    // Customer tier and loyalty
    public CustomerTier Tier { get; private set; }
    public int LoyaltyPoints { get; private set; }

    // Addresses
    private readonly List<CustomerAddress> _addresses = [];
    public IReadOnlyCollection<CustomerAddress> Addresses => _addresses.AsReadOnly();

    private Customer() { } // EF Core

    #region Factory Methods

    public static Customer Create(string fullName, Email email)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new DomainException("Full name is required");

        if (email == null)
            throw new DomainException("Email is required");

        var customer = new Customer
        {
            FullName = fullName.Trim(),
            Email = email,
            Tier = CustomerTier.Standard,
            LoyaltyPoints = 0
        };

        customer.AddEvent(new CustomerCreatedEvent(customer.Id, email.Value, null));
        return customer;
    }

    public static Customer CreateFromGuest(Guest guest, string fullName, Email email)
    {
        if (guest == null)
            throw new DomainException("Guest is required");

        if (string.IsNullOrWhiteSpace(fullName))
            throw new DomainException("Full name is required");

        if (email == null)
            throw new DomainException("Email is required");

        var customer = new Customer
        {
            FullName = fullName.Trim(),
            Email = email,
            Phone = !string.IsNullOrWhiteSpace(guest.Phone) ? new PhoneNumber(guest.Phone) : null,
            Tier = CustomerTier.Standard,
            LoyaltyPoints = 0
        };

        // Copy last shipping address from guest if available
        if (guest.LastShippingAddress != null)
        {
            customer._addresses.Add(new CustomerAddress(
                customer.Id,
                guest.LastShippingAddress,
                AddressType.Home,
                isDefault: true,
                label: "Địa chỉ mặc định"));
        }

        customer.AddEvent(new CustomerCreatedEvent(customer.Id, email.Value, guest.Id));
        return customer;
    }

    #endregion

    #region Profile Methods

    public void UpdateProfile(string fullName, PhoneNumber? phone, DateOnly? dateOfBirth, Gender? gender)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new DomainException("Full name is required");

        FullName = fullName.Trim();
        Phone = phone;
        DateOfBirth = dateOfBirth;
        Gender = gender;

        AddEvent(new CustomerProfileUpdatedEvent(Id));
        IncreaseVersion();
    }

    public void UpdateAvatar(string? avatarUrl)
    {
        AvatarUrl = avatarUrl;
        IncreaseVersion();
    }

    public void UpdateEmail(Email newEmail)
    {
        if (newEmail == null)
            throw new DomainException("Email is required");

        Email = newEmail;
        IncreaseVersion();
    }

    #endregion

    #region Address Methods

    public void AddAddress(Address address, AddressType type = AddressType.Home, bool isDefault = false, string? label = null)
    {
        if (address == null)
            throw new DomainException("Address is required");

        // If this is the first address or set as default, make it default
        var shouldBeDefault = isDefault || !_addresses.Any();

        if (shouldBeDefault)
        {
            // Clear other defaults
            foreach (var addr in _addresses.Where(a => a.IsDefault))
            {
                addr.ClearDefault();
            }
        }

        var customerAddress = new CustomerAddress(Id, address, type, shouldBeDefault, label);
        _addresses.Add(customerAddress);

        AddEvent(new CustomerAddressAddedEvent(Id, customerAddress.Id, shouldBeDefault));
        IncreaseVersion();
    }

    public void RemoveAddress(Guid addressId)
    {
        var address = _addresses.FirstOrDefault(a => a.Id == addressId);
        if (address == null) return;

        var wasDefault = address.IsDefault;
        _addresses.Remove(address);

        // If removed default, set first remaining as default
        if (wasDefault && _addresses.Any())
        {
            _addresses.First().SetAsDefault();
        }

        IncreaseVersion();
    }

    public void SetDefaultAddress(Guid addressId)
    {
        var address = _addresses.FirstOrDefault(a => a.Id == addressId)
            ?? throw new DomainException("Address not found");

        foreach (var addr in _addresses.Where(a => a.IsDefault))
        {
            addr.ClearDefault();
        }

        address.SetAsDefault();
        IncreaseVersion();
    }

    public void UpdateAddress(Guid addressId, Address newAddress, AddressType? type = null, string? label = null)
    {
        var customerAddress = _addresses.FirstOrDefault(a => a.Id == addressId)
            ?? throw new DomainException("Address not found");

        customerAddress.Update(newAddress, type, label);
        IncreaseVersion();
    }

    #endregion

    #region Loyalty Methods

    public void AddLoyaltyPoints(int points)
    {
        if (points <= 0)
            throw new DomainException("Points must be positive");

        LoyaltyPoints += points;
        IncreaseVersion();
    }

    public void DeductLoyaltyPoints(int points)
    {
        if (points <= 0)
            throw new DomainException("Points must be positive");

        if (points > LoyaltyPoints)
            throw new DomainException("Insufficient loyalty points");

        LoyaltyPoints -= points;
        IncreaseVersion();
    }

    public void UpgradeTier(CustomerTier newTier)
    {
        if (newTier <= Tier)
            throw new DomainException("Can only upgrade to a higher tier");

        var oldTier = Tier;
        Tier = newTier;

        AddEvent(new CustomerTierUpgradedEvent(Id, oldTier, newTier));
        IncreaseVersion();
    }

    #endregion

    #region Account Link

    public void LinkAccount(Guid accountId)
    {
        if (accountId == Guid.Empty)
            throw new DomainException("Account ID is required");

        AccountId = accountId;
        IncreaseVersion();
    }

    #endregion
}
