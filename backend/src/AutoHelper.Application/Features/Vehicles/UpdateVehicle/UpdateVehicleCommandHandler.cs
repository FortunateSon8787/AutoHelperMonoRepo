using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using MediatR;

namespace AutoHelper.Application.Features.Vehicles.UpdateVehicle;

public sealed class UpdateVehicleCommandHandler(
    IVehicleRepository vehicles,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateVehicleCommand, Result>
{
    public async Task<Result> Handle(UpdateVehicleCommand request, CancellationToken ct)
    {
        if (currentUser.Id is null)
            return Result.Failure("User is not authenticated.");

        var vehicle = await vehicles.GetByIdAsync(request.Id, ct);
        if (vehicle is null || vehicle.OwnerId != currentUser.Id.Value)
            return Result.Failure("Vehicle not found.");

        vehicle.UpdateDetails(
            brand: request.Brand,
            model: request.Model,
            year: request.Year,
            color: request.Color,
            mileage: request.Mileage);

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
