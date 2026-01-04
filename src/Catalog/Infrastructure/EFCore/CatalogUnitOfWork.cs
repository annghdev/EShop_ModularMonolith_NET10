using Catalog.Application;
using Catalog.Domain;
using Catalog.Infrastructure.EFCore.Repositories;

namespace Catalog.Infrastructure;

public sealed class CatalogUnitOfWork(CatalogDbContext context, ICurrentUser user, IPublisher publisher)
    : BaseUnitOfWork<CatalogDbContext>(context, user, publisher), ICatalogUnitOfWork
{
    private IProductRepository? productRepository;

    public IProductRepository Products => productRepository ??= new ProductRepository(context);
    public DbSet<Brand> Brands => context.Brands;
    public DbSet<Domain.Attribute> Attributes => context.Attributes;
    public DbSet<Category> Categories => context.Categories;
    public DbSet<Collection> Collections => context.Collections;
}
