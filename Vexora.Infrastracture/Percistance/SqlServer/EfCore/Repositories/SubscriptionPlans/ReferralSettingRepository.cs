using Microsoft.EntityFrameworkCore;
using Vexora.Application.Repositories.SubscriptionPlans;
using Vexora.Domain.Entities.SubscriptionPlans;
using Vexora.Infrastracture.Percistance.SqlServer.EfCore.Context;

namespace Vexora.Infrastracture.Percistance.SqlServer.EfCore.Repositories.SubscriptionPlans;

public sealed class ReferralSettingRepository(ApplicationDbContext context) : IReferralSettingRepository
{
    public Task<ReferralSetting?> GetCurrentAsync(CancellationToken cancellationToken = default)
        => context.ReferralSettings.OrderByDescending(x => x.Id).FirstOrDefaultAsync(cancellationToken);

    public async Task AddAsync(ReferralSetting setting, CancellationToken cancellationToken = default)
        => await context.ReferralSettings.AddAsync(setting, cancellationToken);
}
