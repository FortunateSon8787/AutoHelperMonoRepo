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
            return Result<Guid>.Failure("User is not authenticated.");

        var partner = await partners.GetByAccountUserIdAsync(userId, ct);
        if (partner is null)
            return Result<Guid>.Failure("Partner profile not found for this account.");

        if (!partner.IsVerified || !partner.IsActive)
            return Result<Guid>.Failure("Only verified and active partners can create ad campaigns.");

        if (!Enum.TryParse<AdType>(request.Type, ignoreCase: true, out var adType))
            return Result<Guid>.Failure($"Invalid ad type: {request.Type}.");

        if (!Enum.TryParse<PartnerType>(request.TargetCategory, ignoreCase: true, out var targetCategory))
            return Result<Guid>.Failure($"Invalid target category: {request.TargetCategory}.");

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
