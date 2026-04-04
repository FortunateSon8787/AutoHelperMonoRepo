using FluentValidation;

namespace AutoHelper.Application.Features.Admin.ChatbotConfig.UpdateChatbotConfig;

public sealed class UpdateChatbotConfigCommandValidator : AbstractValidator<UpdateChatbotConfigCommand>
{
    public UpdateChatbotConfigCommandValidator()
    {
        RuleFor(x => x.MaxCharsPerField)
            .GreaterThan(0).WithMessage("MaxCharsPerField must be greater than 0.")
            .LessThanOrEqualTo(10_000).WithMessage("MaxCharsPerField must not exceed 10 000.");

        RuleFor(x => x.TopUpPriceUsd)
            .GreaterThan(0).WithMessage("TopUpPriceUsd must be greater than 0.");

        RuleFor(x => x.TopUpRequestCount)
            .GreaterThan(0).WithMessage("TopUpRequestCount must be greater than 0.");

        RuleFor(x => x.DailyLimitByPlan)
            .NotNull().WithMessage("DailyLimitByPlan is required.")
            .Must(d => d.Values.All(v => v >= 0))
            .WithMessage("All daily limit values must be non-negative.");
    }
}
