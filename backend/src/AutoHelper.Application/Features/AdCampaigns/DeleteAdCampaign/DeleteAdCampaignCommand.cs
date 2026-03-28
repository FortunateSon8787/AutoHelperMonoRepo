using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.AdCampaigns.DeleteAdCampaign;

public sealed record DeleteAdCampaignCommand(Guid Id) : IRequest<Result>;
