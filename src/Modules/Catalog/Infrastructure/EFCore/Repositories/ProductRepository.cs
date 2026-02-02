using Catalog.Domain;

namespace Catalog.Infrastructure.EFCore.Repositories;

public class ProductRepository(CatalogDbContext db)
    : BaseRepository<Product, CatalogDbContext>(db), IProductRepository
{
    public override async Task<Product> LoadFullAggregate(Guid id, bool changeTracking = true)
    {
        var query = changeTracking ? dbSet : dbSet.AsNoTracking();

        return await query
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Attributes)
                .ThenInclude(pa => pa.Attribute)
            .Include(p => p.Variants)
                .ThenInclude(v => v.AttributeValues)
                    .ThenInclude(vav => vav.Value)
            .Include(p => p.Variants)
                .ThenInclude(v => v.AttributeValues)
                    .ThenInclude(vav => vav.Value)
            .Include(p => p.Variants)
                .ThenInclude(v => v.AttributeValues)
                    .ThenInclude(vav => vav.ProductAttribute)
                        .ThenInclude(pa => pa.Attribute)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new NotFoundException("Product", id);

    }

    public async Task<Product> LoadFullAggregateBySlug(string slug, bool changeTracking = true)
    {
        var query = changeTracking ? dbSet : dbSet.AsNoTracking();

        return await query
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Attributes)
                .ThenInclude(pa => pa.Attribute)
            .Include(p => p.Variants)
                .ThenInclude(v => v.AttributeValues)
                    .ThenInclude(vav => vav.Value)
            .Include(p => p.Variants)
                .ThenInclude(v => v.AttributeValues)
                    .ThenInclude(vav => vav.ProductAttribute)
            .FirstOrDefaultAsync(p => p.Slug.Value == slug)
            ?? throw new NotFoundException("Product", slug);
    }

    public override async Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var data = await dbSet
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Attributes)
                .ThenInclude(pa => pa.Attribute)
            .Include(p => p.Variants)
                .ThenInclude(v => v.AttributeValues)
                    .ThenInclude(vav => vav.Value)
            .Include(p => p.Variants)
                .ThenInclude(v => v.AttributeValues)
                    .ThenInclude(vav => vav.ProductAttribute).ToListAsync(cancellationToken);

        return data ?? [];
    }

    public IQueryable<Product> AsQueryable() => dbSet;
}
