using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using MediatR;

namespace AutoHelper.Application.Features.Partners.DeactivatePartner;

public sealed class DeactivatePartnerCommandHandler(
    IPartnerRepository partners,
    IUnitOfWork unitOfWork) : IRequestHandler<DeactivatePartnerCommand, Result>
{
    public async Task<Result> Handle(DeactivatePartnerCommand request, CancellationToken ct)
    {
        var partner = await partners.GetByIdAsync(request.PartnerId, ct);

        if (partner is null)
            return Result.Failure("Partner not found.");

        partner.Deactivate();
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
