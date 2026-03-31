using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using MediatR;

namespace AutoHelper.Application.Features.Vehicles.GetVehicleByVin;

public sealed class GetVehicleByVinQueryHandler(
    IVehicleRepository vehicles) : IRequestHandler<GetVehicleByVinQuery, Result<PublicVehicleResponse>>
{
    public async Task<Result<PublicVehicleResponse>> Handle(GetVehicleByVinQuery request, CancellationToken ct)
    {
        var vehicle = await vehicles.GetByVinAsync(request.Vin, ct);

        if (vehicle is null)
            return AppErrors.Vehicle.NotFound;

        return Result<PublicVehicleResponse>.Success(PublicVehicleResponse.FromVehicle(vehicle));
    }
}
