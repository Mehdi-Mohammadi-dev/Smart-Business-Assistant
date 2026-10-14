namespace Vexora.Api.Contracts.Authentications.Subscriptions;

public record SubscriptionContract
(
    string PlanType,
    string Status,
    DateTimeOffset StartAt,
    DateTimeOffset EndAt,
    bool IsActive,
    int RemainingDays);
