using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using MediatR;

namespace AutoHelper.Application.Features.Admin.Partners.GetAdminPartners;

public sealed class GetAdminPartnersQueryHandler(
    IPartnerRepository partners,
    IReviewRepository reviews)
    : IRequestHandler<GetAdminPartnersQuery, Result<AdminPartnerListResponse>>
{
    public async Task<Result<AdminPartnerListResponse>> Handle(
        GetAdminPartnersQuery request, CancellationToken ct)
    {
        var (items, totalCount) = await partners.GetPagedForAdminAsync(
            request.Page, request.PageSize, request.Search, ct);

        var responseItems = new List<AdminPartnerResponse>(items.Count);
        foreach (var partner in items)
        {
            var partnerReviews = await reviews.GetByPartnerIdAsync(partner.Id, ct);
            responseItems.Add(AdminPartnerResponse.FromPartner(partner, partnerReviews));
        }

        return Result<AdminPartnerListResponse>.Success(
            new AdminPartnerListResponse(responseItems, totalCount, request.Page, request.PageSize));
    }
}
