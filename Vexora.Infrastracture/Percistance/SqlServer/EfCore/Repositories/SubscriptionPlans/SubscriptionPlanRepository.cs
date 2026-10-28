using Microsoft.EntityFrameworkCore;
using Vexora.Application.Repositories.SubscriptionPlans;
using Vexora.Domain.Entities.SubscriptionPlans;
using Vexora.Infrastracture.Percistance.SqlServer.EfCore.Context;

namespace Vexora.Infrastracture.Percistance.SqlServer.EfCore.Repositories.SubscriptionPlans;    

public sealed class SubscriptionPlanRepository(ApplicationDbContext context) : ISubscriptionPlanRepository
{
    public async Task AddAsync(SubscriptionPlan plan, CancellationToken ct)
        => await context.SubscriptionPlans.AddAsync(plan, ct);

    public Task<SubscriptionPlan?> FindByIdAsync(long id, CancellationToken ct)
        => context.SubscriptionPlans.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<SubscriptionPlan>> ListAsync(bool onlyActive, CancellationToken ct )
        => await context.SubscriptionPlans
            .Where(x => !onlyActive || x.IsActive)
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Days)
            .ToListAsync(ct);
}
