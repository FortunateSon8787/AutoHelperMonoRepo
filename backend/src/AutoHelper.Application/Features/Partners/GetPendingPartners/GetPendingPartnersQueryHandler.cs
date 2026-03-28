using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using MediatR;

namespace AutoHelper.Application.Features.Partners.GetPendingPartners;

public sealed class GetPendingPartnersQueryHandler(
    IPartnerRepository partners) : IRequestHandler<GetPendingPartnersQuery, Result<IReadOnlyList<PartnerResponse>>>
{
    public async Task<Result<IReadOnlyList<PartnerResponse>>> Handle(GetPendingPartnersQuery request, CancellationToken ct)
    {
        var pending = await partners.GetPendingVerificationAsync(ct);
        var response = pending.Select(PartnerResponse.FromPartner).ToList();
        return Result<IReadOnlyList<PartnerResponse>>.Success(response);
    }
}
