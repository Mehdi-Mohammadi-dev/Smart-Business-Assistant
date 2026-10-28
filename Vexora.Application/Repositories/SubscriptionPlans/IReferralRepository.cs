using Vexora.Domain.Entities.SubscriptionPlans;

namespace Vexora.Application.Repositories.SubscriptionPlans;

public interface IReferralRepository
{
    Task AddAsync(Referral referral, CancellationToken cancellationToken = default);
    Task<bool> ExistsForReferredUserAsync(long referredUserId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Referral>> ListAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<long, int>> GetReferralCountsAsync(CancellationToken cancellationToken = default);
}
