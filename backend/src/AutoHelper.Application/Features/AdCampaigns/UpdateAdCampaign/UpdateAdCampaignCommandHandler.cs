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
            return Result.Failure("User is not authenticated.");

        var partner = await partners.GetByAccountUserIdAsync(userId, ct);
        if (partner is null)
            return Result.Failure("Partner profile not found for this account.");

        var campaign = await campaigns.GetByIdAsync(request.Id, ct);
        if (campaign is null)
            return Result.Failure("Ad campaign not found.");

        if (campaign.PartnerId != partner.Id)
            return Result.Failure("Access denied. This campaign belongs to a different partner.");

        if (!Enum.TryParse<AdType>(request.Type, ignoreCase: true, out var adType))
            return Result.Failure($"Invalid ad type: {request.Type}.");

        if (!Enum.TryParse<PartnerType>(request.TargetCategory, ignoreCase: true, out var targetCategory))
            return Result.Failure($"Invalid target category: {request.TargetCategory}.");

        campaign.Update(adType, targetCategory, request.Content, request.StartsAt, request.EndsAt, request.ShowToAnonymous);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
