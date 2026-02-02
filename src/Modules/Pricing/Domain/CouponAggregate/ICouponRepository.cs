namespace Pricing.Domain;

public interface ICouponRepository : IRepository<Coupon>
{
    Task<Coupon?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IEnumerable<Coupon>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Coupon>> GetExpiredActiveAsync(DateTimeOffset currentTime, CancellationToken cancellationToken = default);
    Task<bool> ExistsCodeAsync(string code, CancellationToken cancellationToken = default);
}
