using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.AdCampaigns.CreateAdCampaign;

public sealed record CreateAdCampaignCommand(
    string Type,
    string TargetCategory,
    string Content,
    DateTime StartsAt,
    DateTime EndsAt,
    bool ShowToAnonymous) : IRequest<Result<Guid>>;
