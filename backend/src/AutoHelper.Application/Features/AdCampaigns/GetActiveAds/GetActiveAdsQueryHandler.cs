using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Domain.Partners;
using MediatR;

namespace AutoHelper.Application.Features.AdCampaigns.GetActiveAds;

public sealed class GetActiveAdsQueryHandler(
    IAdCampaignRepository campaigns) : IRequestHandler<GetActiveAdsQuery, Result<IReadOnlyList<AdCampaignResponse>>>
{
    public async Task<Result<IReadOnlyList<AdCampaignResponse>>> Handle(GetActiveAdsQuery request, CancellationToken ct)
    {
        PartnerType? targetCategory = null;
        if (!string.IsNullOrWhiteSpace(request.TargetCategory))
        {
            if (!Enum.TryParse<PartnerType>(request.TargetCategory, ignoreCase: true, out var parsed))
                return Result<IReadOnlyList<AdCampaignResponse>>.Failure(
                    $"Invalid target category: {request.TargetCategory}.");

            targetCategory = parsed;
        }

        var activeCampaigns = await campaigns.GetActiveForDisplayAsync(
            isAuthenticated: request.IsAuthenticated,
            targetCategory: targetCategory,
            ct: ct);

        // Apply per-campaign visibility rules (partner exclusion, anonymous rules)
        // and perform rotation (random shuffle for equal-priority campaigns)
        IReadOnlyList<AdCampaignResponse> result = activeCampaigns
            .Where(c => c.IsVisibleTo(request.IsAuthenticated, request.IsPartner))
            .OrderBy(_ => Guid.NewGuid()) // rotation: random order per request
            .Select(AdCampaignResponse.FromCampaign)
            .ToList();

        return Result<IReadOnlyList<AdCampaignResponse>>.Success(result);
    }
}
