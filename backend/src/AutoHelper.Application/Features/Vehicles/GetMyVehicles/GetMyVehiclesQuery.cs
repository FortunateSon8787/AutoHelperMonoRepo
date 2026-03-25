using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.Vehicles.GetMyVehicles;

public sealed record GetMyVehiclesQuery : IRequest<Result<IReadOnlyList<VehicleResponse>>>;
