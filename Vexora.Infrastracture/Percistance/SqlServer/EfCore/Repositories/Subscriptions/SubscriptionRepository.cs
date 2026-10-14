using Microsoft.EntityFrameworkCore;
using Vexora.Application.Repositories.Subscriptions;
using Vexora.Domain.Entities.Sunscriptions;
using Vexora.Infrastracture.Percistance.SqlServer.EfCore.Context;

namespace Vexora.Infrastracture.Percistance.SqlServer.EfCore.Repositories.Subscriptions;

public sealed class SubscriptionRepository(ApplicationDbContext context) : ISubscriptionRepository
{
    public async Task AddAsync(Subscription subscription, CancellationToken cancellationToken = default)
        => await context.Subscriptions.AddAsync(subscription, cancellationToken);

    public Task<Subscription?> GetLatestByUserIdAsync(long userId, CancellationToken cancellationToken = default)
        => context.Subscriptions
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.EndAt)
            .FirstOrDefaultAsync(cancellationToken);
}
