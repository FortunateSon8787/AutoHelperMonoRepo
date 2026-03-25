using AutoHelper.Domain.Vehicles;

namespace AutoHelper.Application.Features.Vehicles;

public sealed record VehicleResponse(
    Guid Id,
    string Vin,
    string Brand,
    string Model,
    int Year,
    string? Color,
    int Mileage,
    string Status,
    Guid OwnerId,
    string? PartnerName,
    string? DocumentUrl)
{
    public static VehicleResponse FromVehicle(Vehicle vehicle) =>
        new(vehicle.Id,
            vehicle.Vin,
            vehicle.Brand,
            vehicle.Model,
            vehicle.Year,
            vehicle.Color,
            vehicle.Mileage,
            vehicle.Status.ToString(),
            vehicle.OwnerId,
            vehicle.PartnerName,
            vehicle.DocumentUrl);
}
