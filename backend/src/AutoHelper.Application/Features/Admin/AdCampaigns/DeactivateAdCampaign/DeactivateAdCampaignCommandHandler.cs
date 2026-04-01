using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using MediatR;

namespace AutoHelper.Application.Features.Admin.AdCampaigns.DeactivateAdCampaign;

public sealed class DeactivateAdCampaignCommandHandler(
    IAdCampaignRepository adCampaigns,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeactivateAdCampaignCommand, Result>
{
    public async Task<Result> Handle(DeactivateAdCampaignCommand request, CancellationToken ct)
    {
        var campaign = await adCampaigns.GetByIdAsync(request.Id, ct);
        if (campaign is null || campaign.IsDeleted)
            return Result.Failure(AppErrors.Admin.AdCampaignNotFound);

        if (!campaign.IsActive)
            return Result.Failure(AppErrors.Admin.AdCampaignAlreadyInactive);

        campaign.Deactivate();
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
