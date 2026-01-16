using Kernel.Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public abstract class BaseRepository<TEntity, TDbContext>(TDbContext db) : IRepository<TEntity>
    where TEntity : class, IAggregate
    where TDbContext : BaseDbContext
{
    protected readonly DbSet<TEntity> dbSet = db.Set<TEntity>();

    public void Add(TEntity entity)
    {
        dbSet.Add(entity);
    }

    public void Update(TEntity entity)
    {
        dbSet.Update(entity);
    }

    public void Remove(TEntity entity)
    {
        dbSet.Remove(entity);
    }

    public async Task<bool> CheckExistsAsync(Guid id)
    {
        return await dbSet.AnyAsync(x => x.Id == id);
    }

    public abstract Task<TEntity> LoadFullAggregate(Guid id, bool changeTracking = true);

    public abstract Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
}
