using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.Admin.AdCampaigns.ActivateAdCampaign;

public sealed record ActivateAdCampaignCommand(Guid Id) : IRequest<Result>;
