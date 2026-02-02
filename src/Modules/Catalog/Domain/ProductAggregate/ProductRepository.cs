namespace Catalog.Domain;

public interface IProductRepository : IRepository<Product>
{
    Task<Product> LoadFullAggregateBySlug(string slug, bool changeTracking = true);
    IQueryable<Product> AsQueryable();
}
