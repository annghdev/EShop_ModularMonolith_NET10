using Contracts.Requests.Catalog;
using Contracts.Responses;

namespace BlazorAdmin.Services;

public interface IBrandService
{
    Task<List<BrandDto>> GetBrandsAsync();
    Task<Guid> CreateBrandAsync(CreateBrandRequest request);
    Task UpdateBrandAsync(Guid id, UpdateBrandRequest request);
    Task DeleteBrandAsync(Guid id);
}
