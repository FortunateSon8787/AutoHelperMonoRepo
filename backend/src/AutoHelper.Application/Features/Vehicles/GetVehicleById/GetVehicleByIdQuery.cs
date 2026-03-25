using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.Vehicles.GetVehicleById;

public sealed record GetVehicleByIdQuery(Guid Id) : IRequest<Result<VehicleResponse>>;
