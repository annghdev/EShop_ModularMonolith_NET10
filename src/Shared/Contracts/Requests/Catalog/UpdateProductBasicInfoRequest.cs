using Contracts.Responses;

namespace Contracts.Requests.Catalog;

public class UpdateProductBasicInfoRequest
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? SkuPrefix { get; set; }
    public string? Description { get; set; }
    public DimensionsDto Dimensions { get; set; } = new(0, 0, 0, 0);
}
