using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using MediatR;

namespace AutoHelper.Application.Features.Vehicles.GetVehicleOwner;

public sealed class GetVehicleOwnerQueryHandler(
    IVehicleRepository vehicles,
    ICustomerRepository customers) : IRequestHandler<GetVehicleOwnerQuery, Result<VehicleOwnerResponse>>
{
    public async Task<Result<VehicleOwnerResponse>> Handle(GetVehicleOwnerQuery request, CancellationToken ct)
    {
        var vehicle = await vehicles.GetByVinAsync(request.Vin, ct);
        if (vehicle is null)
            return AppErrors.Vehicle.NotFound;

        var owner = await customers.GetByIdAsync(vehicle.OwnerId, ct);
        if (owner is null)
            return AppErrors.Vehicle.OwnerNotFound;

        return Result<VehicleOwnerResponse>.Success(VehicleOwnerResponse.FromCustomer(owner));
    }
}
