using AutoHelper.Domain.Customers;
using FluentValidation;

namespace AutoHelper.Application.Features.Admin.SubscriptionPlans.UpdatePlanConfig;

public sealed class UpdatePlanConfigCommandValidator : AbstractValidator<UpdatePlanConfigCommand>
{
    private static readonly string[] AllowedPlans =
        Enum.GetNames<SubscriptionPlan>()
            .Where(name => name != nameof(SubscriptionPlan.None))
            .ToArray();

    public UpdatePlanConfigCommandValidator()
    {
        RuleFor(x => x.Plan)
            .NotEmpty().WithMessage("Plan is required.")
            .Must(plan => AllowedPlans.Contains(plan, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Plan must be one of: {string.Join(", ", AllowedPlans)}.");

        RuleFor(x => x.PriceUsd)
            .GreaterThan(0).WithMessage("Price must be greater than 0.")
            .LessThanOrEqualTo(9999.99m).WithMessage("Price must not exceed $9,999.99.")
            .PrecisionScale(6, 2, false).WithMessage("Price must have at most 2 decimal places.");

        RuleFor(x => x.MonthlyQuota)
            .GreaterThan(0).WithMessage("Monthly quota must be greater than 0.")
            .LessThanOrEqualTo(10_000).WithMessage("Monthly quota must not exceed 10,000 requests.");
    }
}
