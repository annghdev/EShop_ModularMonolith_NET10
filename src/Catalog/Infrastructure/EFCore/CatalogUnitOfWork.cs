using Catalog.Application;
using Catalog.Domain;
using Catalog.Domain.CategoryAggregate;
using Catalog.Infrastructure.EFCore.Repositories;

namespace Catalog.Infrastructure;

public sealed class CatalogUnitOfWork(CatalogDbContext context, ICurrentUser user, IPublisher publisher)
    : BaseUnitOfWork<CatalogDbContext>(context, user, publisher), ICatalogUnitOfWork
{
    private IProductRepository? productRepository;
    private ICategoryRepository? categoryRepository;

    public IProductRepository Products => productRepository ??= new ProductRepository(context);
    public ICategoryRepository Categories => categoryRepository ??= new CategoryRepository(context);
    public DbSet<Brand> Brands => context.Brands;
    public DbSet<Domain.Attribute> Attributes => context.Attributes;
    public DbSet<Collection> Collections => context.Collections;
}
