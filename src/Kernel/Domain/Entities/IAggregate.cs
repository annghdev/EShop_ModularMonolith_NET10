namespace Kernel.Domain;

public interface IAggregate : IEntity
{
    long Version { get; }
    IReadOnlyCollection<DomainEvent> Events { get; }
    void AddEvent(DomainEvent evt);
    void RemoveEvent(DomainEvent evt);
    void ClearEvents();
}
