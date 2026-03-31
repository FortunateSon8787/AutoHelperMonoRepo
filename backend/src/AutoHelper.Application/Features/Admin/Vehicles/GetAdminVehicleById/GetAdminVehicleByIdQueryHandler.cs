using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using MediatR;

namespace AutoHelper.Application.Features.Admin.Vehicles.GetAdminVehicleById;

public sealed class GetAdminVehicleByIdQueryHandler(IVehicleRepository vehicles)
    : IRequestHandler<GetAdminVehicleByIdQuery, Result<AdminVehicleResponse>>
{
    public async Task<Result<AdminVehicleResponse>> Handle(
        GetAdminVehicleByIdQuery request, CancellationToken ct)
    {
        var vehicle = await vehicles.GetByIdAsync(request.Id, ct);
        if (vehicle is null)
            return Result<AdminVehicleResponse>.Failure(AppErrors.Admin.VehicleNotFound);

        return Result<AdminVehicleResponse>.Success(AdminVehicleResponse.FromVehicle(vehicle));
    }
}
