using FluentValidation;

namespace AutoHelper.Application.Features.AdCampaigns.UpdateAdCampaign;

public sealed class UpdateAdCampaignCommandValidator : AbstractValidator<UpdateAdCampaignCommand>
{
    public UpdateAdCampaignCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Campaign ID is required.");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Ad type is required.");

        RuleFor(x => x.TargetCategory)
            .NotEmpty().WithMessage("Target category is required.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required.")
            .MaximumLength(2048).WithMessage("Content must not exceed 2048 characters.");

        RuleFor(x => x.EndsAt)
            .GreaterThan(x => x.StartsAt).WithMessage("End date must be after start date.");
    }
}
