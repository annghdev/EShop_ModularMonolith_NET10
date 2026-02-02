namespace Kernel.Domain;

public interface IRepository<T>
    where T : class, IAggregate
{
    void Add(T entity);
    void Update(T entity);
    void Remove(T entity);
    Task<T> LoadFullAggregate(Guid id, bool changeTracking = true);
    Task<bool> CheckExistsAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
}
