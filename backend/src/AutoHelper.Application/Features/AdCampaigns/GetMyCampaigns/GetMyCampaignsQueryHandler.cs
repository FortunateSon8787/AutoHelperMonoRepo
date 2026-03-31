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
            return AppErrors.Auth.NotAuthenticated;

        var partner = await partners.GetByAccountUserIdAsync(userId, ct);
        if (partner is null)
            return AppErrors.Partner.ProfileNotFoundForAccount;

        var partnerCampaigns = await campaigns.GetByPartnerIdAsync(partner.Id, ct);

        IReadOnlyList<AdCampaignResponse> result = partnerCampaigns
            .Select(AdCampaignResponse.FromCampaign)
            .ToList();

        return Result<IReadOnlyList<AdCampaignResponse>>.Success(result);
    }
}
