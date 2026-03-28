using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using MediatR;

namespace AutoHelper.Application.Features.AdCampaigns.DeleteAdCampaign;

public sealed class DeleteAdCampaignCommandHandler(
    IAdCampaignRepository campaigns,
    IPartnerRepository partners,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteAdCampaignCommand, Result>
{
    public async Task<Result> Handle(DeleteAdCampaignCommand request, CancellationToken ct)
    {
        if (currentUser.Id is not { } userId)
            return Result.Failure("User is not authenticated.");

        var partner = await partners.GetByAccountUserIdAsync(userId, ct);
        if (partner is null)
            return Result.Failure("Partner profile not found for this account.");

        var campaign = await campaigns.GetByIdAsync(request.Id, ct);
        if (campaign is null)
            return Result.Failure("Ad campaign not found.");

        if (campaign.PartnerId != partner.Id)
            return Result.Failure("Access denied. This campaign belongs to a different partner.");

        campaign.Delete();
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
