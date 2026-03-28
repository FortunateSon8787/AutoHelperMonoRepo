using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using MediatR;

namespace AutoHelper.Application.Features.AdCampaigns.GetMyCampaigns;

public sealed class GetMyCampaignsQueryHandler(
    IAdCampaignRepository campaigns,
    IPartnerRepository partners,
    ICurrentUser currentUser) : IRequestHandler<GetMyCampaignsQuery, Result<IReadOnlyList<AdCampaignResponse>>>
{
    public async Task<Result<IReadOnlyList<AdCampaignResponse>>> Handle(GetMyCampaignsQuery request, CancellationToken ct)
    {
        if (currentUser.Id is not { } userId)
            return Result<IReadOnlyList<AdCampaignResponse>>.Failure("User is not authenticated.");

        var partner = await partners.GetByAccountUserIdAsync(userId, ct);
        if (partner is null)
            return Result<IReadOnlyList<AdCampaignResponse>>.Failure("Partner profile not found for this account.");

        var partnerCampaigns = await campaigns.GetByPartnerIdAsync(partner.Id, ct);

        IReadOnlyList<AdCampaignResponse> result = partnerCampaigns
            .Select(AdCampaignResponse.FromCampaign)
            .ToList();

        return Result<IReadOnlyList<AdCampaignResponse>>.Success(result);
    }
}
