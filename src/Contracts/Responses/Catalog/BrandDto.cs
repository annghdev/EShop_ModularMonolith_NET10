namespace Contracts.Responses;

public class BrandDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Logo { get; set; }
}
