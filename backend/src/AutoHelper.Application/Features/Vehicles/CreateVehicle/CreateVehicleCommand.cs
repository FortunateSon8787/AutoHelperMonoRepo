using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.Vehicles.CreateVehicle;

public sealed record CreateVehicleCommand(
    string Vin,
    string Brand,
    string Model,
    int Year,
    string? Color,
    int Mileage) : IRequest<Result<Guid>>;
