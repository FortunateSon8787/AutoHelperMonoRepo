using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using MediatR;

namespace AutoHelper.Application.Features.Vehicles.GetMyVehicles;

public sealed class GetMyVehiclesQueryHandler(
    IVehicleRepository vehicles,
    ICurrentUser currentUser) : IRequestHandler<GetMyVehiclesQuery, Result<IReadOnlyList<VehicleResponse>>>
{
    public async Task<Result<IReadOnlyList<VehicleResponse>>> Handle(
        GetMyVehiclesQuery request, CancellationToken ct)
    {
        if (currentUser.Id is null)
            return Result<IReadOnlyList<VehicleResponse>>.Failure("User is not authenticated.");

        var ownerVehicles = await vehicles.GetAllByOwnerIdAsync(currentUser.Id.Value, ct);

        IReadOnlyList<VehicleResponse> response = ownerVehicles
            .Select(VehicleResponse.FromVehicle)
            .ToList();

        return Result<IReadOnlyList<VehicleResponse>>.Success(response);
    }
}
