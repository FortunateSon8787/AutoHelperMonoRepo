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
    public static SubscriptionResponse FromCustomer(Customer customer) => new(
        Status: customer.SubscriptionStatus.ToString(),
        Plan: customer.SubscriptionPlan.ToString(),
        StartDate: customer.SubscriptionStartDate,
        EndDate: customer.SubscriptionEndDate,
        AiRequestsRemaining: customer.AiRequestsRemaining,
        MonthlyPriceUsd: PriceForPlan(customer.SubscriptionPlan),
        MonthlyRequestQuota: customer.SubscriptionPlan == SubscriptionPlan.None
            ? null
            : Customer.MonthlyRequestsForPlan(customer.SubscriptionPlan));

    private static decimal? PriceForPlan(SubscriptionPlan plan) => plan switch
    {
        SubscriptionPlan.Normal => 4.99m,
        SubscriptionPlan.Pro    => 7.99m,
        SubscriptionPlan.Max    => 12.99m,
        _                       => null
    };
}
