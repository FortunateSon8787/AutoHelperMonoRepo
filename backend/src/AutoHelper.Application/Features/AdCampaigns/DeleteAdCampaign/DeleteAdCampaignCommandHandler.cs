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
            return AppErrors.Auth.NotAuthenticated;

        var partner = await partners.GetByAccountUserIdAsync(userId, ct);
        if (partner is null)
            return AppErrors.Partner.ProfileNotFoundForAccount;

        var campaign = await campaigns.GetByIdAsync(request.Id, ct);
        if (campaign is null)
            return AppErrors.AdCampaign.NotFound;

        if (campaign.PartnerId != partner.Id)
            return AppErrors.AdCampaign.AccessDenied;

        campaign.Delete();
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
