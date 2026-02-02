namespace Kernel.Domain;

public abstract class AggregateRoot : AuditableEntity, IAggregate
{
    public long Version { get; private set; }

    private readonly List<DomainEvent> _events = [];
    public IReadOnlyCollection<DomainEvent> Events => _events.AsReadOnly();

    public void AddEvent(DomainEvent e) => _events.Add(e);
    public void RemoveEvent(DomainEvent e) => _events.Remove(e);
    public void ClearEvents() => _events.Clear();

    protected void IncreaseVersion() => Version++;
}
