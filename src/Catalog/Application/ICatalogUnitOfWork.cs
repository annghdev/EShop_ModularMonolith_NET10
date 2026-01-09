using Catalog.Domain;
using Catalog.Domain.CategoryAggregate;
using Kernel.Application.Interfaces;

namespace Catalog.Application;

public interface ICatalogUnitOfWork : IUnitOfWork
{
    public IProductRepository Products { get; }
    public ICategoryRepository Categories { get; }
    public DbSet<Brand> Brands { get; }
    public DbSet<Domain.Attribute> Attributes { get; }
    public DbSet<Collection> Collections { get; }
}
