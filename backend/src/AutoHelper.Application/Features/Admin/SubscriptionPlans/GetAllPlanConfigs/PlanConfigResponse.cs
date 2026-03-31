using AutoHelper.Domain.Customers;

namespace AutoHelper.Application.Features.Admin.SubscriptionPlans.GetAllPlanConfigs;

public sealed record PlanConfigResponse(
    Guid Id,
    string Plan,
    decimal PriceUsd,
    int MonthlyQuota)
{
    public static PlanConfigResponse FromConfig(SubscriptionPlanConfig config) => new(
        Id: config.Id,
        Plan: config.Plan.ToString(),
        PriceUsd: config.PriceUsd,
        MonthlyQuota: config.MonthlyQuota);
}
