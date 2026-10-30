using Vexora.Domain.Entities.Sunscriptions;

namespace Vexora.Application.Repositories.Subscriptions;

public interface ISubscriptionRepository
{
    Task AddAsync(Subscription subscription, CancellationToken cancellationToken = default);

    Task<Subscription?> GetLatestByUserIdAsync(long userId, CancellationToken cancellationToken = default);

    Task<Subscription?> GetLatestPurchasedByUserIdAsync(long userId, CancellationToken cancellationToken = default);
}
