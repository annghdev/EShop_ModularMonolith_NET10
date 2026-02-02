namespace Kernel.Domain;

public interface ISoftDelete
{
    bool IsDeleted { get; set; }
}
