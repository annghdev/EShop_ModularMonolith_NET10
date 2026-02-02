namespace Kernel.Application;

public interface IUserContext
{
    Guid UserId { get; }
    string? Email { get; }
    string? FullName { get; }
    IEnumerable<string> Roles { get; }
    bool IsAuthenticated { get; }
}
