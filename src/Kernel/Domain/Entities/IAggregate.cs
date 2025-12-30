namespace Kernel.Domain;

public interface IAggregate : IEntity, IUserTracking, ISoftDelete
{
}
