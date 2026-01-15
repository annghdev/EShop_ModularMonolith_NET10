namespace Users.Domain;

// Guest events
public record GuestCreatedEvent(Guid Id, string GuestId) : DomainEvent;
public record GuestConvertedEvent(Guid GuestEntityId, string GuestId, Guid CustomerId) : DomainEvent;
