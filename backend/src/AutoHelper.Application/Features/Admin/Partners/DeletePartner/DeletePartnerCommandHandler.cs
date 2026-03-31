using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using MediatR;

namespace AutoHelper.Application.Features.Admin.Partners.DeletePartner;

public sealed class DeletePartnerCommandHandler(
    IPartnerRepository partners,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeletePartnerCommand, Result>
{
    public async Task<Result> Handle(DeletePartnerCommand request, CancellationToken ct)
    {
        var partner = await partners.GetByIdAsync(request.PartnerId, ct);
        if (partner is null)
            return Result.Failure(AppErrors.Admin.PartnerNotFound);

        partner.Delete();
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
