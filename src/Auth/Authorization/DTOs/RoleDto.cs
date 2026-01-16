namespace Auth.Authorization;

public class RoleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
}
