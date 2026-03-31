using AutoHelper.Domain.Customers;

namespace AutoHelper.Application.Common.Interfaces;

public interface ISubscriptionPlanConfigRepository
{
    Task<IReadOnlyList<SubscriptionPlanConfig>> GetAllAsync(CancellationToken ct = default);
    Task<SubscriptionPlanConfig?> GetByPlanAsync(SubscriptionPlan plan, CancellationToken ct = default);
}
