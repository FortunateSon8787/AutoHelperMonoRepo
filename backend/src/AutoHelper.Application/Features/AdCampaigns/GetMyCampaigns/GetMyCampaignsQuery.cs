using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.AdCampaigns.GetMyCampaigns;

public sealed record GetMyCampaignsQuery : IRequest<Result<IReadOnlyList<AdCampaignResponse>>>;
