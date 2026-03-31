using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.Admin.SubscriptionPlans.GetAllPlanConfigs;

public sealed record GetAllPlanConfigsQuery : IRequest<Result<IReadOnlyList<PlanConfigResponse>>>;
