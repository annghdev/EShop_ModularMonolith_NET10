using MediatR;

namespace Contracts;

public abstract record IntegrationEvent : INotification
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}