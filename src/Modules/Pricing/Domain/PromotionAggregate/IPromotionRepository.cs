namespace Pricing.Domain;

public interface IPromotionRepository : IRepository<Promotion>
{
    Task<IEnumerable<Promotion>> GetActiveAsync(DateTimeOffset currentTime, CancellationToken cancellationToken = default);
    Task<IEnumerable<Promotion>> GetByTypeAsync(PromotionType type, CancellationToken cancellationToken = default);
    Task<IEnumerable<Promotion>> GetActiveByTypeAsync(PromotionType type, DateTimeOffset currentTime, CancellationToken cancellationToken = default);
    Task<IEnumerable<Promotion>> GetExpiredActiveAsync(DateTimeOffset currentTime, CancellationToken cancellationToken = default);
}
