using FluentValidation;

namespace AutoHelper.Application.Features.AdCampaigns.CreateAdCampaign;

public sealed class CreateAdCampaignCommandValidator : AbstractValidator<CreateAdCampaignCommand>
{
    public CreateAdCampaignCommandValidator()
    {
        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Ad type is required.");

        RuleFor(x => x.TargetCategory)
            .NotEmpty().WithMessage("Target category is required.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required.")
            .MaximumLength(2048).WithMessage("Content must not exceed 2048 characters.");

        RuleFor(x => x.StartsAt)
            .NotEmpty().WithMessage("Start date is required.");

        RuleFor(x => x.EndsAt)
            .NotEmpty().WithMessage("End date is required.")
            .GreaterThan(x => x.StartsAt).WithMessage("End date must be after start date.");
    }
}
