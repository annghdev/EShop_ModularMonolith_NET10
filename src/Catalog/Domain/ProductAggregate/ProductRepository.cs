namespace Catalog.Domain;

public interface IProductRepository : IRepository<Product>
{
    Task<Product> GetAggregateBySlug(string slug);
}
