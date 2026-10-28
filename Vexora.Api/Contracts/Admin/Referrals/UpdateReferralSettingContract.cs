namespace Vexora.Api.Contracts.Admin.Referrals;

public sealed record UpdateReferralSettingContract(bool IsEnabled, int RewardDays, int MaxRewardDaysPerUser);
