using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Domain.Vehicles;
using MediatR;

namespace AutoHelper.Application.Features.Vehicles.CreateVehicle;

public sealed class CreateVehicleCommandHandler(
    IVehicleRepository vehicles,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateVehicleCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateVehicleCommand request, CancellationToken ct)
    {
        if (currentUser.Id is null)
            return Result<Guid>.Failure("User is not authenticated.");

        if (await vehicles.ExistsByVinAsync(request.Vin, ct))
            return Result<Guid>.Failure("A vehicle with this VIN already exists.");

        var vehicle = Vehicle.Create(
            vin: request.Vin,
            brand: request.Brand,
            model: request.Model,
            year: request.Year,
            ownerId: currentUser.Id.Value,
            color: request.Color,
            mileage: request.Mileage);

        vehicles.Add(vehicle);
        await unitOfWork.SaveChangesAsync(ct);

        return Result<Guid>.Success(vehicle.Id);
    }
}
