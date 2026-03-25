using FluentValidation;

namespace AutoHelper.Application.Features.Vehicles.CreateVehicle;

public sealed class CreateVehicleCommandValidator : AbstractValidator<CreateVehicleCommand>
{
    public CreateVehicleCommandValidator()
    {
        RuleFor(x => x.Vin)
            .NotEmpty().WithMessage("VIN is required.")
            .Must(vin => System.Text.RegularExpressions.Regex.IsMatch(
                vin.Trim().ToUpperInvariant(),
                @"^[A-HJ-NPR-Z0-9]{17}$"))
            .WithMessage("VIN must be exactly 17 alphanumeric characters (I, O, Q are not allowed).")
            .When(x => !string.IsNullOrWhiteSpace(x.Vin));

        RuleFor(x => x.Brand)
            .NotEmpty().WithMessage("Brand is required.")
            .MaximumLength(128).WithMessage("Brand must not exceed 128 characters.");

        RuleFor(x => x.Model)
            .NotEmpty().WithMessage("Model is required.")
            .MaximumLength(128).WithMessage("Model must not exceed 128 characters.");

        RuleFor(x => x.Year)
            .InclusiveBetween(1900, DateTime.UtcNow.Year + 1)
            .WithMessage($"Year must be between 1900 and {DateTime.UtcNow.Year + 1}.");

        RuleFor(x => x.Mileage)
            .GreaterThanOrEqualTo(0).WithMessage("Mileage must be a non-negative number.");

        RuleFor(x => x.Color)
            .MaximumLength(64).WithMessage("Color must not exceed 64 characters.")
            .When(x => x.Color is not null);
    }
}
