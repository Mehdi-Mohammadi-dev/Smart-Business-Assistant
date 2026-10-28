using Vexora.Domain.sharedKernel.BaseEntities;

namespace Vexora.Domain.Entities.SubscriptionPlans; 

public class Referral : BaseEntity
{
    public long ReferrerUserId { get; private set; }
    public long ReferredUserId { get; private set; }
    public bool RewardGranted { get; private set; }
    public DateTimeOffset? RewardedAt { get; private set; }
    public int RewardDays { get; private set; }

    private Referral() { }

    public static Referral Create(long referrerId, long referredId)
    {
        if (referrerId == referredId) throw new ArgumentException("معرفی خود مجاز نیست.");
        return new Referral { ReferrerUserId = referrerId, ReferredUserId = referredId };
    }

    public void GrantReward(int days)
    {
        if (RewardGranted) return;
        RewardGranted = true;
        RewardDays = days;
        RewardedAt = DateTimeOffset.UtcNow;
    }
}
