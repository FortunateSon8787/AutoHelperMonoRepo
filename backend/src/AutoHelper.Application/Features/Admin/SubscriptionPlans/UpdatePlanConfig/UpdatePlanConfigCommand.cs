using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.Admin.SubscriptionPlans.UpdatePlanConfig;

public sealed record UpdatePlanConfigCommand(
    string Plan,
    decimal PriceUsd,
    int MonthlyQuota) : IRequest<Result>;
