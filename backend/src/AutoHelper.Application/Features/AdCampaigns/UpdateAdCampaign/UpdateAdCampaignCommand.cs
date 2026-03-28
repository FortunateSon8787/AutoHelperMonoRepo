using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.AdCampaigns.UpdateAdCampaign;

public sealed record UpdateAdCampaignCommand(
    Guid Id,
    string Type,
    string TargetCategory,
    string Content,
    DateTime StartsAt,
    DateTime EndsAt,
    bool ShowToAnonymous) : IRequest<Result>;
