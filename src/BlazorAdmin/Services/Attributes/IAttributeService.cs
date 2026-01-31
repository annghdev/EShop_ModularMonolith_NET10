using Contracts.Requests.Catalog;
using Contracts.Responses;

namespace BlazorAdmin.Services;

public interface IAttributeService
{
    Task<List<AttributeDto>> GetAttributesAsync();
    Task<AttributeDto?> GetAttributeByIdAsync(Guid id);
    Task<Guid> CreateAttributeAsync(CreateAttributeRequest request);
    Task UpdateAttributeAsync(Guid id, UpdateAttributeRequest request);
    Task DeleteAttributeAsync(Guid id);
    Task<Guid> AddAttributeValueAsync(Guid attributeId, AddAttributeValueRequest request);
    Task UpdateAttributeValueAsync(Guid attributeId, Guid valueId, UpdateAttributeValueRequest request);
    Task RemoveAttributeValueAsync(Guid attributeId, Guid valueId);
}
