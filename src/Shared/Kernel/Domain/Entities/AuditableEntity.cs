namespace Kernel.Domain;

public abstract class AuditableEntity : BaseEntity, IUserTracking, ISoftDelete
{
    public string CreatedBy { get; set; } = "system";
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
}
