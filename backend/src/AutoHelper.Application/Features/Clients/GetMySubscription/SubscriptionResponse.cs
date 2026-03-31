using AutoHelper.Domain.Customers;

namespace AutoHelper.Application.Features.Clients.GetMySubscription;

public sealed record SubscriptionResponse(
    string Status,
    string Plan,
    DateTime? StartDate,
    DateTime? EndDate,
    int AiRequestsRemaining,
    decimal? MonthlyPriceUsd,
    int? MonthlyRequestQuota)
{
    public static SubscriptionResponse FromCustomer(Customer customer, SubscriptionPlanConfig? config) => new(
        Status: customer.SubscriptionStatus.ToString(),
        Plan: customer.SubscriptionPlan.ToString(),
        StartDate: customer.SubscriptionStartDate,
        EndDate: customer.SubscriptionEndDate,
        AiRequestsRemaining: customer.AiRequestsRemaining,
        MonthlyPriceUsd: config?.PriceUsd,
        MonthlyRequestQuota: config?.MonthlyQuota);
}
