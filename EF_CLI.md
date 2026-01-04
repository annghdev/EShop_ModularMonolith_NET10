### migration at API

Add-Migration InitCatalogDb -Project Catalog -StartupProject API -Context CatalogDbContext -OutputDir Infrastructure/EFCore/Migrations

### at module

Add-Migration InitCatalogDb -Context CatalogDbContext -OutputDir Infrastructure/EFCore/Migrations
