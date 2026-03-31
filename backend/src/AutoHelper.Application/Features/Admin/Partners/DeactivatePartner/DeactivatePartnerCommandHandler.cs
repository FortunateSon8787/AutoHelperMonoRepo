using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using MediatR;

namespace AutoHelper.Application.Features.Admin.Partners.DeactivatePartner;

public sealed class DeactivatePartnerCommandHandler(
    IPartnerRepository partners,
    IAdCampaignRepository adCampaigns,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeactivatePartnerCommand, Result>
{
    public async Task<Result> Handle(DeactivatePartnerCommand request, CancellationToken ct)
    {
        var partner = await partners.GetByIdAsync(request.PartnerId, ct);
        if (partner is null)
            return Result.Failure(AppErrors.Admin.PartnerNotFound);

        if (!partner.IsActive)
            return Result.Failure(AppErrors.Admin.PartnerAlreadyDeactivated);

        partner.Deactivate();

        var activeCampaigns = await adCampaigns.GetActiveByPartnerIdAsync(partner.Id, ct);
        foreach (var campaign in activeCampaigns)
            campaign.Deactivate();

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
