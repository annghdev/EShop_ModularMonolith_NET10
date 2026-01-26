namespace Users.Domain;

// Guest events
public record GuestCreatedEvent(Guid GuestId, string ClientId) : DomainEvent;
public record GuestConvertedEvent(Guid GuestEntityId, string GuestId, Guid CustomerId) : DomainEvent;
