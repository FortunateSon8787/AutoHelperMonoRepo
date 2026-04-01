using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.Admin.AdCampaigns.GetAdminAdCampaigns;

public sealed record GetAdminAdCampaignsQuery(int Page, int PageSize, Guid? PartnerId)
    : IRequest<Result<AdminAdCampaignListResponse>>;
