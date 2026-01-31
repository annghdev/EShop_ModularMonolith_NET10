using Contracts.Requests.Catalog;
using Contracts.Responses;

namespace BlazorAdmin.Services;

public interface ICategoryService
{
    Task<List<CategoryTreeDto>> GetCategoryTreeAsync();
    Task<CategoryDetailDto?> GetCategoryDetailAsync(Guid id);
    Task<Guid> CreateCategoryAsync(CreateCategoryRequest request);
    Task UpdateCategoryAsync(Guid id, UpdateCategoryRequest request);
    Task DeleteCategoryAsync(Guid id);
}
