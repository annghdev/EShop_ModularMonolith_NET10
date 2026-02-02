namespace Users.Domain;

// Customer events
public record CustomerCreatedEvent(Guid CustomerId, string Email, Guid? FromGuestId) : DomainEvent;
public record CustomerProfileUpdatedEvent(Guid CustomerId) : DomainEvent;
public record CustomerTierUpgradedEvent(Guid CustomerId, CustomerTier OldTier, CustomerTier NewTier) : DomainEvent;
public record CustomerAddressAddedEvent(Guid CustomerId, Guid AddressId, bool IsDefault) : DomainEvent;
