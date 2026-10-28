using Vexora.Domain.sharedKernel.BaseEntities;

namespace Vexora.Domain.Entities.SubscriptionPlans; 

public class SubscriptionPlan : BaseEntity
{
    public string Name { get; private set; } = null!;
    public int Days { get; private set; }
    public long PriceToman { get; private set; }
    public int DiscountPercent { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsFeatured { get; private set; }
    public int SortOrder { get; private set; }


    public long FinalPriceToman => PriceToman - PriceToman * DiscountPercent / 100;

    private SubscriptionPlan() { }

    public static SubscriptionPlan Create(string name, int days, long priceToman, int discountPercent, bool isActive, bool isFeatured, int sortOrder)
    {
        var p = new SubscriptionPlan();
        p.Update(name, days, priceToman, discountPercent, isActive, isFeatured, sortOrder);
        return p;
    }

    public void Update(string name, int days, long priceToman, int discountPercent, bool isActive, bool isFeatured, int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 100)
            throw new ArgumentException("نام پلن نامعتبر است.");
        if (days is < 1 or > 3650) throw new ArgumentException("تعداد روز نامعتبر است.");
        if (priceToman < 0) throw new ArgumentException("قیمت نامعتبر است.");
        if (discountPercent is < 0 or > 100) throw new ArgumentException("درصد تخفیف نامعتبر است.");
        Name = name.Trim();
        Days = days;
        PriceToman = priceToman;
        DiscountPercent = discountPercent;
        IsActive = isActive;
        IsFeatured = isFeatured;
        SortOrder = sortOrder;
    }

    public void Deactivate() => IsActive = false;
}
