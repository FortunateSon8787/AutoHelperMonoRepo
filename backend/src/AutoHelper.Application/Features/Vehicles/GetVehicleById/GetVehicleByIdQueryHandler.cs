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
            return AppErrors.Auth.NotAuthenticated;

        var vehicle = await vehicles.GetByIdAsync(request.Id, ct);
        if (vehicle is null || vehicle.OwnerId != currentUser.Id.Value)
            return AppErrors.Vehicle.NotFound;

        return Result<VehicleResponse>.Success(VehicleResponse.FromVehicle(vehicle));
    }
}
