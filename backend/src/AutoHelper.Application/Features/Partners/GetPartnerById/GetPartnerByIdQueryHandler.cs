using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using MediatR;

namespace AutoHelper.Application.Features.Partners.GetPartnerById;

public sealed class GetPartnerByIdQueryHandler(
    IPartnerRepository partners)
    : IRequestHandler<GetPartnerByIdQuery, Result<PartnerResponse>>
{
    public async Task<Result<PartnerResponse>> Handle(GetPartnerByIdQuery request, CancellationToken ct)
    {
        var partner = await partners.GetByIdAsync(request.PartnerId, ct);

        if (partner is null || !partner.IsVerified || !partner.IsActive)
            return Result<PartnerResponse>.Failure("Partner not found.");

        return Result<PartnerResponse>.Success(PartnerResponse.FromPartner(partner));
    }
}
