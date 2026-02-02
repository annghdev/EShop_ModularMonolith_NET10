namespace Contracts;

public record AccountRegiteredForcustomerIntegrationEvent(
    Guid AccountId,
    string? GuestId,
    string? FullName,
    string Email,
    string? PhoneNumber) : IntegrationEvent;
