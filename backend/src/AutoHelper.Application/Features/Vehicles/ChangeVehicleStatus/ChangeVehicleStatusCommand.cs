using AutoHelper.Domain.Vehicles;
using MediatR;
using AppResult = AutoHelper.Application.Common.Result;

namespace AutoHelper.Application.Features.Vehicles.ChangeVehicleStatus;

public sealed record ChangeVehicleStatusCommand(
    Guid Id,
    VehicleStatus Status,
    string? PartnerName,
    string? DocumentUrl) : IRequest<AppResult>;
