using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.Vehicles.GetVehicleOwner;

/// <summary>
/// Returns the public profile of the owner of a vehicle identified by VIN.
/// </summary>
public sealed record GetVehicleOwnerQuery(string Vin) : IRequest<Result<VehicleOwnerResponse>>;
