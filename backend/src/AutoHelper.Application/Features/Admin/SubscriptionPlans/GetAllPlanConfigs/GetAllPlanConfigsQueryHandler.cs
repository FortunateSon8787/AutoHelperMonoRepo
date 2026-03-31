using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using MediatR;

namespace AutoHelper.Application.Features.Admin.SubscriptionPlans.GetAllPlanConfigs;

public sealed class GetAllPlanConfigsQueryHandler(
    ISubscriptionPlanConfigRepository planConfigs)
    : IRequestHandler<GetAllPlanConfigsQuery, Result<IReadOnlyList<PlanConfigResponse>>>
{
    public async Task<Result<IReadOnlyList<PlanConfigResponse>>> Handle(
        GetAllPlanConfigsQuery request, CancellationToken ct)
    {
        var configs = await planConfigs.GetAllAsync(ct);
        var response = configs.Select(PlanConfigResponse.FromConfig).ToList();
        return Result<IReadOnlyList<PlanConfigResponse>>.Success(response);
    }
}
