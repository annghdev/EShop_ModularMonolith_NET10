namespace Kernel.Domain;

public abstract class AggregateRoot : AuditableEntity, IAggregate
{
    public long Version { get; set; } = DateTime.Now.Ticks;


    private readonly List<BaseDomainEvent> _events = [];
    public IReadOnlyCollection<BaseDomainEvent> Events => _events.AsReadOnly();

    public void AddEvent(BaseDomainEvent e) => _events.Add(e);
    public void RemoveEvent(BaseDomainEvent e) => _events.Remove(e);
    public void ClearEvents() => _events.Clear();
}
