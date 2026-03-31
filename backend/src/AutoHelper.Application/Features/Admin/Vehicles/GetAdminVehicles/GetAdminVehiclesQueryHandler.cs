using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using MediatR;

namespace AutoHelper.Application.Features.Admin.Vehicles.GetAdminVehicles;

public sealed class GetAdminVehiclesQueryHandler(IVehicleRepository vehicles)
    : IRequestHandler<GetAdminVehiclesQuery, Result<AdminVehicleListResponse>>
{
    public async Task<Result<AdminVehicleListResponse>> Handle(
        GetAdminVehiclesQuery request, CancellationToken ct)
    {
        var (items, totalCount) = await vehicles.GetPagedAsync(
            request.Page, request.PageSize, request.Search, ct);

        var responseItems = items
            .Select(AdminVehicleResponse.FromVehicle)
            .ToList();

        return Result<AdminVehicleListResponse>.Success(
            new AdminVehicleListResponse(responseItems, totalCount, request.Page, request.PageSize));
    }
}
