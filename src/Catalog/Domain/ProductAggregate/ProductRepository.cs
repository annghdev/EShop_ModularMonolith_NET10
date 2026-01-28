namespace Catalog.Domain;

public interface IProductRepository : IRepository<Product>
{
    Task<Product> LoadFullAggregateBySlug(string slug);
    IQueryable<Product> AsQueryable();
}
