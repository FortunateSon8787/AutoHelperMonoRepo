using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.Admin.Vehicles.GetAdminVehicleById;

public sealed record GetAdminVehicleByIdQuery(Guid Id)
    : IRequest<Result<AdminVehicleResponse>>;
