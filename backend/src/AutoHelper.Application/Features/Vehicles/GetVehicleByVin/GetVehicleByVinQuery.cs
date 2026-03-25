using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.Vehicles.GetVehicleByVin;

/// <summary>
/// Returns the public details of a vehicle identified by VIN. No authentication required.
/// </summary>
public sealed record GetVehicleByVinQuery(string Vin) : IRequest<Result<PublicVehicleResponse>>;
