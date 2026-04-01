using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using MediatR;

namespace AutoHelper.Application.Features.Admin.AdCampaigns.GetAdminAdCampaigns;

public sealed class GetAdminAdCampaignsQueryHandler(IAdCampaignRepository adCampaigns)
    : IRequestHandler<GetAdminAdCampaignsQuery, Result<AdminAdCampaignListResponse>>
{
    public async Task<Result<AdminAdCampaignListResponse>> Handle(
        GetAdminAdCampaignsQuery request, CancellationToken ct)
    {
        var (items, totalCount) = await adCampaigns.GetPagedForAdminAsync(
            request.Page, request.PageSize, request.PartnerId, ct);

        var responseItems = items
            .Select(AdminAdCampaignResponse.FromCampaign)
            .ToList();

        return Result<AdminAdCampaignListResponse>.Success(
            new AdminAdCampaignListResponse(responseItems, totalCount, request.Page, request.PageSize));
    }
}
