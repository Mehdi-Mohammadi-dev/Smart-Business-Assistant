using Microsoft.EntityFrameworkCore;
using Vexora.Application.Repositories.SubscriptionPlans;
using Vexora.Domain.Entities.SubscriptionPlans;
using Vexora.Infrastracture.Percistance.SqlServer.EfCore.Context;

namespace Vexora.Infrastracture.Percistance.SqlServer.EfCore.Repositories.SubscriptionPlans;

public sealed class ReferralRepository(ApplicationDbContext context) : IReferralRepository
{
    public async Task AddAsync(Referral referral, CancellationToken cancellationToken = default)
        => await context.Referrals.AddAsync(referral, cancellationToken);

    public Task<bool> ExistsForReferredUserAsync(long referredUserId, CancellationToken cancellationToken = default)
        => context.Referrals.AnyAsync(x => x.ReferredUserId == referredUserId, cancellationToken);

    public async Task<IReadOnlyList<Referral>> ListAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        => await context.Referrals.AsNoTracking().OrderByDescending(x => x.CreateAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Referral>> ListByReferrerAsync(long referrerUserId, int page, int pageSize, CancellationToken cancellationToken = default)
        => await context.Referrals.AsNoTracking()
            .Where(x => x.ReferrerUserId == referrerUserId)
            .OrderByDescending(x => x.CreateAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyDictionary<long, int>> GetReferralCountsAsync(CancellationToken cancellationToken = default)
        => await context.Referrals.AsNoTracking().GroupBy(x => x.ReferrerUserId)
            .Select(group => new { UserId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(x => x.UserId, x => x.Count, cancellationToken);
}
