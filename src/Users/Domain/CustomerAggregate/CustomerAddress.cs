namespace Users.Domain;

/// <summary>
/// Customer address entity - stores customer's saved addresses
/// </summary>
public class CustomerAddress : BaseEntity
{
    public Guid CustomerId { get; private set; }
    public Customer? Customer { get; private set; }

    public Address Address { get; private set; }
    public AddressType Type { get; private set; }
    public bool IsDefault { get; private set; }
    public string? Label { get; private set; }  // "Nhà riêng", "Công ty", etc.

    private CustomerAddress() { } // EF Core

    internal CustomerAddress(Guid customerId, Address address, AddressType type, bool isDefault, string? label = null)
    {
        if (customerId == Guid.Empty)
            throw new DomainException("Customer ID is required");

        if (address == null)
            throw new DomainException("Address is required");

        CustomerId = customerId;
        Address = address;
        Type = type;
        IsDefault = isDefault;
        Label = label;
    }

    internal void SetAsDefault()
    {
        IsDefault = true;
    }

    internal void ClearDefault()
    {
        IsDefault = false;
    }

    internal void Update(Address newAddress, AddressType? type = null, string? label = null)
    {
        if (newAddress != null)
            Address = newAddress;

        if (type.HasValue)
            Type = type.Value;

        if (label != null)
            Label = label;
    }
}
