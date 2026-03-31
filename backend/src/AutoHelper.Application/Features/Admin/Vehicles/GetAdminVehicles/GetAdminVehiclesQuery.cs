using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.Admin.Vehicles.GetAdminVehicles;

public sealed record GetAdminVehiclesQuery(int Page, int PageSize, string? Search)
    : IRequest<Result<AdminVehicleListResponse>>;
