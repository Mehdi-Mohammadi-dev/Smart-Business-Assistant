using Vexora.Domain.Entities.SubscriptionPlans;

namespace Vexora.Application.Dtos.SubscriptionPlans;

public sealed record SaveSubscriptionPlanDto(
    string Name,
    int Days,
    long PriceToman,
    int DiscountPercent,
    bool IsActive,
    bool IsFeatured,
    int SortOrder);

public sealed record SubscriptionPlanDto(
    long Id,
    string Name,
    int Days,
    long PriceToman,
    int DiscountPercent,
    long FinalPriceToman,
    long PricePerDayToman,
    bool IsActive,
    bool IsFeatured,
    int SortOrder)
{
    public static SubscriptionPlanDto From(SubscriptionPlan p) => new(
        p.Id, p.Name, p.Days, p.PriceToman, p.DiscountPercent,
        p.FinalPriceToman, p.Days == 0 ? 0 : p.FinalPriceToman / p.Days,
        p.IsActive, p.IsFeatured, p.SortOrder);
}
