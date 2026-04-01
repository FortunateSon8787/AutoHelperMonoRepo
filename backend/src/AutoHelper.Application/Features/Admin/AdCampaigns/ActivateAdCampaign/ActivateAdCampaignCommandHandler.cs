using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using MediatR;

namespace AutoHelper.Application.Features.Admin.AdCampaigns.ActivateAdCampaign;

public sealed class ActivateAdCampaignCommandHandler(
    IAdCampaignRepository adCampaigns,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ActivateAdCampaignCommand, Result>
{
    public async Task<Result> Handle(ActivateAdCampaignCommand request, CancellationToken ct)
    {
        var campaign = await adCampaigns.GetByIdAsync(request.Id, ct);
        if (campaign is null || campaign.IsDeleted)
            return Result.Failure(AppErrors.Admin.AdCampaignNotFound);

        if (campaign.IsActive)
            return Result.Failure(AppErrors.Admin.AdCampaignAlreadyActive);

        campaign.Activate();
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
