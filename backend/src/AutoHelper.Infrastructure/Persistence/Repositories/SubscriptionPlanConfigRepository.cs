using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Domain.Customers;
using Microsoft.EntityFrameworkCore;

namespace AutoHelper.Infrastructure.Persistence.Repositories;

public sealed class SubscriptionPlanConfigRepository(AppDbContext db) : ISubscriptionPlanConfigRepository
{
    public Task<IReadOnlyList<SubscriptionPlanConfig>> GetAllAsync(CancellationToken ct) =>
        db.SubscriptionPlanConfigs
            .OrderBy(c => c.Plan)
            .ToListAsync(ct)
            .ContinueWith<IReadOnlyList<SubscriptionPlanConfig>>(t => t.Result, ct);

    public Task<SubscriptionPlanConfig?> GetByPlanAsync(SubscriptionPlan plan, CancellationToken ct) =>
        db.SubscriptionPlanConfigs.FirstOrDefaultAsync(c => c.Plan == plan, ct);
}
