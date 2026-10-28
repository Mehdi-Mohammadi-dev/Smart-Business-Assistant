namespace Vexora.Api.Contracts.Admin.Referrals;

public sealed record ReferralSettingResponse(bool IsEnabled, int RewardDays, int MaxRewardDaysPerUser);
