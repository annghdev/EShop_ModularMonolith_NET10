namespace Users.Domain;

/// <summary>
/// Guest aggregate - represents anonymous visitor with temporary data
/// </summary>
public class Guest : AggregateRoot
{
    public string GuestId { get; private set; } = string.Empty;  // Client-generated ID (cookie/localStorage)
    public string? Email { get; private set; }                    // Captured but not verified
    public string? Phone { get; private set; }
    public string? FullName { get; private set; }

    // Tracking
    public Address? LastShippingAddress { get; private set; }
    public DateTimeOffset FirstVisitAt { get; private set; }
    public DateTimeOffset LastActivityAt { get; private set; }

    // Conversion to registered customer
    public bool IsConverted { get; private set; }
    public Guid? ConvertedToCustomerId { get; private set; }

    private Guest() { } // EF Core

    #region Factory Methods

    public static Guest Create(string guestId)
    {
        if (string.IsNullOrWhiteSpace(guestId))
            throw new DomainException("Guest ID is required");

        var guest = new Guest
        {
            GuestId = guestId,
            FirstVisitAt = DateTimeOffset.UtcNow,
            LastActivityAt = DateTimeOffset.UtcNow,
            IsConverted = false
        };

        guest.AddEvent(new GuestCreatedEvent(guest.Id, guestId));
        return guest;
    }

    #endregion

    #region Methods

    public void UpdateInfo(string? email, string? phone, string? fullName)
    {
        Email = email?.ToLowerInvariant();
        Phone = phone;
        FullName = fullName?.Trim();
        RecordActivity();
    }

    public void SetLastShippingAddress(Address address)
    {
        LastShippingAddress = address ?? throw new DomainException("Address is required");
        RecordActivity();
    }

    public void MarkAsConverted(Guid customerId)
    {
        if (customerId == Guid.Empty)
            throw new DomainException("Customer ID is required");

        if (IsConverted)
            throw new DomainException("Guest has already been converted");

        IsConverted = true;
        ConvertedToCustomerId = customerId;

        AddEvent(new GuestConvertedEvent(Id, GuestId, customerId));
        IncreaseVersion();
    }

    public void RecordActivity()
    {
        LastActivityAt = DateTimeOffset.UtcNow;
        IncreaseVersion();
    }

    #endregion
}
