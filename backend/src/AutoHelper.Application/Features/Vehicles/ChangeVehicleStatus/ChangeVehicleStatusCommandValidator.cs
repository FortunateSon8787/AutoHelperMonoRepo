using AutoHelper.Domain.Vehicles;
using FluentValidation;

namespace AutoHelper.Application.Features.Vehicles.ChangeVehicleStatus;

public sealed class ChangeVehicleStatusCommandValidator : AbstractValidator<ChangeVehicleStatusCommand>
{
    public ChangeVehicleStatusCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Vehicle ID is required.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid vehicle status.");

        RuleFor(x => x.PartnerName)
            .NotEmpty().WithMessage("Partner name is required when status is InRepair.")
            .MaximumLength(256).WithMessage("Partner name must not exceed 256 characters.")
            .When(x => x.Status == VehicleStatus.InRepair);

        RuleFor(x => x.DocumentUrl)
            .NotEmpty().WithMessage("Document URL is required when status is Recycled or Dismantled.")
            .MaximumLength(1024).WithMessage("Document URL must not exceed 1024 characters.")
            .When(x => x.Status is VehicleStatus.Recycled or VehicleStatus.Dismantled);
    }
}
