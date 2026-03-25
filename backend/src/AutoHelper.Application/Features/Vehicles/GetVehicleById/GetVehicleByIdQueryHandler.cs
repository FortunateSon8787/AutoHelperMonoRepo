using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using MediatR;

namespace AutoHelper.Application.Features.Vehicles.GetVehicleById;

public sealed class GetVehicleByIdQueryHandler(
    IVehicleRepository vehicles,
    ICurrentUser currentUser) : IRequestHandler<GetVehicleByIdQuery, Result<VehicleResponse>>
{
    public async Task<Result<VehicleResponse>> Handle(GetVehicleByIdQuery request, CancellationToken ct)
    {
        if (currentUser.Id is null)
            return Result<VehicleResponse>.Failure("User is not authenticated.");

        var vehicle = await vehicles.GetByIdAsync(request.Id, ct);
        if (vehicle is null || vehicle.OwnerId != currentUser.Id.Value)
            return Result<VehicleResponse>.Failure("Vehicle not found.");

        return Result<VehicleResponse>.Success(VehicleResponse.FromVehicle(vehicle));
    }
}
