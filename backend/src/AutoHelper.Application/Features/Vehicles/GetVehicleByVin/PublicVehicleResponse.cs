using AutoHelper.Domain.Vehicles;

namespace AutoHelper.Application.Features.Vehicles.GetVehicleByVin;

/// <summary>
/// Public-facing vehicle details, safe to expose without authentication.
/// Excludes owner identity and sensitive document links.
/// </summary>
public sealed record PublicVehicleResponse(
    string Vin,
    string Brand,
    string Model,
    int Year,
    string? Color,
    int Mileage,
    string Status,
    string? PartnerName)
{
    public static PublicVehicleResponse FromVehicle(Vehicle vehicle) =>
        new(vehicle.Vin,
            vehicle.Brand,
            vehicle.Model,
            vehicle.Year,
            vehicle.Color,
            vehicle.Mileage,
            vehicle.Status.ToString(),
            vehicle.PartnerName);
}
