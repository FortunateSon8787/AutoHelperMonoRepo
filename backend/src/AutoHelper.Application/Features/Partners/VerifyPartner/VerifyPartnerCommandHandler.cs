using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using MediatR;

namespace AutoHelper.Application.Features.Partners.VerifyPartner;

public sealed class VerifyPartnerCommandHandler(
    IPartnerRepository partners,
    IUnitOfWork unitOfWork) : IRequestHandler<VerifyPartnerCommand, Result>
{
    public async Task<Result> Handle(VerifyPartnerCommand request, CancellationToken ct)
    {
        var partner = await partners.GetByIdAsync(request.PartnerId, ct);

        if (partner is null)
            return Result.Failure("Partner not found.");

        partner.Verify();
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
