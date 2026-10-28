using Vexora.Domain.sharedKernel.BaseEntities;

namespace Vexora.Domain.Entities.SubscriptionPlans; 


public class ReferralSetting : BaseEntity
{
    public bool IsEnabled { get; private set; }
    public int RewardDays { get; private set; }
    public int MaxRewardDaysPerUser { get; private set; }

    private ReferralSetting() { }

    public static ReferralSetting Create(bool enabled, int rewardDays, int maxRewardDays)
    {
        var s = new ReferralSetting();
        s.Update(enabled, rewardDays, maxRewardDays);
        return s;
    }

    public void Update(bool enabled, int rewardDays, int maxRewardDays)
    {
        if (rewardDays < 0 || maxRewardDays < 0) throw new ArgumentException("مقدار نامعتبر است.");
        IsEnabled = enabled;
        RewardDays = rewardDays;
        MaxRewardDaysPerUser = maxRewardDays;
    }
}
