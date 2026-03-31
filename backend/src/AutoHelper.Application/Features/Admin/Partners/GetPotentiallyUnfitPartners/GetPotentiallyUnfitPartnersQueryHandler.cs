using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using MediatR;

namespace AutoHelper.Application.Features.Admin.Partners.GetPotentiallyUnfitPartners;

public sealed class GetPotentiallyUnfitPartnersQueryHandler(
    IPartnerRepository partners,
    IReviewRepository reviews)
    : IRequestHandler<GetPotentiallyUnfitPartnersQuery, Result<IReadOnlyList<AdminUnfitPartnerResponse>>>
{
    public async Task<Result<IReadOnlyList<AdminUnfitPartnerResponse>>> Handle(
        GetPotentiallyUnfitPartnersQuery request, CancellationToken ct)
    {
        var unfitPartners = await partners.GetPotentiallyUnfitAsync(ct);

        var result = new List<AdminUnfitPartnerResponse>(unfitPartners.Count);
        foreach (var partner in unfitPartners)
        {
            var lowRatingReviews = await reviews.GetLowRatingByPartnerIdAsync(partner.Id, ct);
            result.Add(AdminUnfitPartnerResponse.FromPartner(partner, lowRatingReviews));
        }

        return Result<IReadOnlyList<AdminUnfitPartnerResponse>>.Success(result);
    }
}
