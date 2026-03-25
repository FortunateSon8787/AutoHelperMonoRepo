using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.Vehicles.UpdateVehicle;

public sealed record UpdateVehicleCommand(
    Guid Id,
    string Brand,
    string Model,
    int Year,
    string? Color,
    int Mileage) : IRequest<Result>;
