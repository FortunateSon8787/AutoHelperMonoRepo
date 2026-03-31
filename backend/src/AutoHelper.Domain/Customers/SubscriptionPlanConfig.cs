using AutoHelper.Domain.Common;

namespace AutoHelper.Domain.Customers;

/// <summary>
/// Configurable metadata for a subscription plan tier — price and monthly AI request quota.
/// One record per paid SubscriptionPlan (Normal / Pro / Max).
/// </summary>
public sealed class SubscriptionPlanConfig : Entity<Guid>
{
    public SubscriptionPlan Plan { get; private set; }
    public decimal PriceUsd { get; private set; }
    public int MonthlyQuota { get; private set; }

    // Required for EF Core
    private SubscriptionPlanConfig() { }

    private SubscriptionPlanConfig(Guid id, SubscriptionPlan plan, decimal priceUsd, int monthlyQuota)
        : base(id)
    {
        Plan = plan;
        PriceUsd = priceUsd;
        MonthlyQuota = monthlyQuota;
    }

    public static SubscriptionPlanConfig Create(SubscriptionPlan plan, decimal priceUsd, int monthlyQuota)
        => new(Guid.NewGuid(), plan, priceUsd, monthlyQuota);

    public void Update(decimal priceUsd, int monthlyQuota)
    {
        PriceUsd = priceUsd;
        MonthlyQuota = monthlyQuota;
    }
}
