using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using MediatR;

namespace AutoHelper.Application.Features.Partners.GetMyPartnerProfile;

public sealed class GetMyPartnerProfileQueryHandler(
    IPartnerRepository partners,
    ICurrentUser currentUser) : IRequestHandler<GetMyPartnerProfileQuery, Result<PartnerResponse>>
{
    public async Task<Result<PartnerResponse>> Handle(GetMyPartnerProfileQuery request, CancellationToken ct)
    {
        if (currentUser.Id is not { } userId)
            return AppErrors.Auth.NotAuthenticated;

        var partner = await partners.GetByAccountUserIdAsync(userId, ct);

        if (partner is null)
            return AppErrors.Partner.ProfileNotFound;

        return Result<PartnerResponse>.Success(PartnerResponse.FromPartner(partner));
    }
}
