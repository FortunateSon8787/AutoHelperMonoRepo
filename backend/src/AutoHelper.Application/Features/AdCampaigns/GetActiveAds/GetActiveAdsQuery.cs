using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.AdCampaigns.GetActiveAds;

/// <summary>
/// Returns active ad campaigns eligible for display on the current page.
/// Applies targeting by service category and visibility rules (anonymous / partner).
/// </summary>
public sealed record GetActiveAdsQuery(
    bool IsAuthenticated,
    bool IsPartner,
    string? TargetCategory) : IRequest<Result<IReadOnlyList<AdCampaignResponse>>>;
