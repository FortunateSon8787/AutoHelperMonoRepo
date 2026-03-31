using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Domain.AdCampaigns;
using AutoHelper.Domain.Partners;
using MediatR;

namespace AutoHelper.Application.Features.AdCampaigns.CreateAdCampaign;

public sealed class CreateAdCampaignCommandHandler(
    IAdCampaignRepository campaigns,
    IPartnerRepository partners,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateAdCampaignCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateAdCampaignCommand request, CancellationToken ct)
    {
        if (currentUser.Id is not { } userId)
            return AppErrors.Auth.NotAuthenticated;

        var partner = await partners.GetByAccountUserIdAsync(userId, ct);
        if (partner is null)
            return AppErrors.Partner.ProfileNotFoundForAccount;

        if (!partner.IsVerified || !partner.IsActive)
            return AppErrors.Partner.NotVerifiedOrInactive;

        if (!Enum.TryParse<AdType>(request.Type, ignoreCase: true, out var adType))
            return AppErrors.AdCampaign.InvalidAdType;

        if (!Enum.TryParse<PartnerType>(request.TargetCategory, ignoreCase: true, out var targetCategory))
            return AppErrors.AdCampaign.InvalidTargetCategory;

        var campaign = AdCampaign.Create(
            partnerId: partner.Id,
            type: adType,
            targetCategory: targetCategory,
            content: request.Content,
            startsAt: request.StartsAt,
            endsAt: request.EndsAt,
            showToAnonymous: request.ShowToAnonymous);

        campaigns.Add(campaign);
        await unitOfWork.SaveChangesAsync(ct);

        return Result<Guid>.Success(campaign.Id);
    }
}
