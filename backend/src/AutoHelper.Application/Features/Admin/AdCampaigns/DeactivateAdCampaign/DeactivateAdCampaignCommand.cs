using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.Admin.AdCampaigns.DeactivateAdCampaign;

public sealed record DeactivateAdCampaignCommand(Guid Id) : IRequest<Result>;
