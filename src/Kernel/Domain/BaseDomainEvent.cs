using MediatR;

namespace Kernel.Domain;

public abstract record BaseDomainEvent : INotification
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public DateTimeOffset OccuredAt { get; set; } = DateTimeOffset.UtcNow;
}