using Catalog.Domain;
using Catalog.Domain.CategoryAggregate;
using Kernel.Infrastructure.EFCore;

namespace Catalog.Infrastructure.EFCore.Repositories;

public class CategoryRepository(CatalogDbContext db) : BaseRepository<Category, CatalogDbContext>(db), ICategoryRepository
{

    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbSet.FindAsync([id], cancellationToken);
    }

    public async Task<Category?> GetByIdWithHierarchyAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // First, collect all category IDs in the hierarchy
        var hierarchyIds = new List<Guid>();
        var currentId = id;

        while (currentId != Guid.Empty)
        {
            hierarchyIds.Add(currentId);
            var parentInfo = await dbSet
                .Where(c => c.Id == currentId)
                .Select(c => new { c.ParentId })
                .FirstOrDefaultAsync(cancellationToken);

            currentId = parentInfo?.ParentId ?? Guid.Empty;

            // Prevent infinite loops in case of circular references
            if (hierarchyIds.Contains(currentId))
                break;
        }

        // Load all categories in the hierarchy at once
        var categories = await dbSet
            .Include(c => c.DefaultAttributes)
                .ThenInclude(da => da.Attribute)
            .Where(c => hierarchyIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, cancellationToken);

        // Rebuild the hierarchy relationships
        foreach (var cat in categories.Values)
        {
            if (cat.ParentId.HasValue && categories.ContainsKey(cat.ParentId.Value))
            {
                cat.Parent = categories[cat.ParentId.Value];
            }
        }

        return categories.GetValueOrDefault(id);
    }

    public override async Task<IEnumerable<Category>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbSet.ToListAsync(cancellationToken);
    }

    public override Task<Category> LoadFullAggregate(Guid id, bool changeTracking = true)
    {
        return GetByIdWithHierarchyAsync(id);
    }
}
