using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using MediatR;

namespace AutoHelper.Application.Features.Admin.Partners.GetAdminPartnerById;

public sealed class GetAdminPartnerByIdQueryHandler(
    IPartnerRepository partners,
    IReviewRepository reviews)
    : IRequestHandler<GetAdminPartnerByIdQuery, Result<AdminPartnerDetailResponse>>
{
    public async Task<Result<AdminPartnerDetailResponse>> Handle(
        GetAdminPartnerByIdQuery request, CancellationToken ct)
    {
        var partner = await partners.GetByIdAsync(request.PartnerId, ct);
        if (partner is null)
            return Result<AdminPartnerDetailResponse>.Failure(AppErrors.Admin.PartnerNotFound);

        var partnerReviews = await reviews.GetByPartnerIdAsync(partner.Id, ct);
        return Result<AdminPartnerDetailResponse>.Success(
            AdminPartnerDetailResponse.FromPartner(partner, partnerReviews));
    }
}
