namespace Vexora.Application.Dtos.Authentications.Subscriptions;

public record SubscriptionDto(
    string PlanType,
    string Status,
    DateTimeOffset StartAt,
    DateTimeOffset EndAt,
    bool IsActive,
    int RemainingDays);
