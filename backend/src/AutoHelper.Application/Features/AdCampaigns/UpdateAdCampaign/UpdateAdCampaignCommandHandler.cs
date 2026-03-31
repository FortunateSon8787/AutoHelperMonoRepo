using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Domain.AdCampaigns;
using AutoHelper.Domain.Partners;
using MediatR;

namespace AutoHelper.Application.Features.AdCampaigns.UpdateAdCampaign;

public sealed class UpdateAdCampaignCommandHandler(
    IAdCampaignRepository campaigns,
    IPartnerRepository partners,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateAdCampaignCommand, Result>
{
    public async Task<Result> Handle(UpdateAdCampaignCommand request, CancellationToken ct)
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

        if (!Enum.TryParse<AdType>(request.Type, ignoreCase: true, out var adType))
            return AppErrors.AdCampaign.InvalidAdType;

        if (!Enum.TryParse<PartnerType>(request.TargetCategory, ignoreCase: true, out var targetCategory))
            return AppErrors.AdCampaign.InvalidTargetCategory;

        campaign.Update(adType, targetCategory, request.Content, request.StartsAt, request.EndsAt, request.ShowToAnonymous);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
