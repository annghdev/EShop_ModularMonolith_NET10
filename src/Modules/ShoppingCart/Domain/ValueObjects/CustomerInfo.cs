namespace ShoppingCart.Domain;

/// <summary>
/// Identifies the cart owner - either a registered customer or a guest
/// </summary>
public class CustomerInfo : BaseValueObject
{
    public Guid? CustomerId { get; }
    public string? GuestId { get; }

    public bool IsGuest => CustomerId == null;
    public bool IsRegistered => CustomerId != null;

    private CustomerInfo() { }

    private CustomerInfo(Guid? customerId, string? guestId)
    {
        CustomerId = customerId;
        GuestId = guestId;
    }

    public static CustomerInfo ForRegisteredCustomer(Guid customerId)
    {
        if (customerId == Guid.Empty)
            throw new DomainException("Customer ID cannot be empty");

        return new CustomerInfo(customerId, null);
    }

    public static CustomerInfo ForGuest(string guestId)
    {
        if (string.IsNullOrWhiteSpace(guestId))
            throw new DomainException("Guest ID cannot be empty");

        return new CustomerInfo(null, guestId);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return CustomerId;
        yield return GuestId;
    }
}
