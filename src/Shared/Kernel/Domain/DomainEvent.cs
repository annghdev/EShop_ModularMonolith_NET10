using MediatR;

namespace Kernel.Domain;

public abstract record DomainEvent : INotification
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;
}