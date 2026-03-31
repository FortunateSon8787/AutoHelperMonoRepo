using AutoHelper.Domain.Customers;
using FluentValidation;

namespace AutoHelper.Application.Features.Clients.ActivateSubscription;

public sealed class ActivateSubscriptionCommandValidator : AbstractValidator<ActivateSubscriptionCommand>
{
    private static readonly string[] AllowedPlans =
        Enum.GetNames<SubscriptionPlan>()
            .Where(name => name != nameof(SubscriptionPlan.None))
            .ToArray();

    public ActivateSubscriptionCommandValidator()
    {
        RuleFor(x => x.Plan)
            .NotEmpty().WithMessage("Plan is required.")
            .Must(plan => AllowedPlans.Contains(plan, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Plan must be one of: {string.Join(", ", AllowedPlans)}.");
    }
}
