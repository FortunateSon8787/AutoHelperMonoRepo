using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Domain.Customers;
using MediatR;

namespace AutoHelper.Application.Features.Admin.SubscriptionPlans.UpdatePlanConfig;

public sealed class UpdatePlanConfigCommandHandler(
    ISubscriptionPlanConfigRepository planConfigs,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdatePlanConfigCommand, Result>
{
    public async Task<Result> Handle(UpdatePlanConfigCommand request, CancellationToken ct)
    {
        if (!Enum.TryParse<SubscriptionPlan>(request.Plan, ignoreCase: true, out var plan)
            || plan == SubscriptionPlan.None)
            return AppErrors.Subscription.InvalidPlan;

        var config = await planConfigs.GetByPlanAsync(plan, ct);
        if (config is null)
            return AppErrors.Subscription.InvalidPlan;

        config.Update(request.PriceUsd, request.MonthlyQuota);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
