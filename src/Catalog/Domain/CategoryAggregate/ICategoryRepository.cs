namespace Catalog.Domain.CategoryAggregate;

public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Category?> GetByIdWithHierarchyAsync(Guid id, CancellationToken cancellationToken = default);
}
