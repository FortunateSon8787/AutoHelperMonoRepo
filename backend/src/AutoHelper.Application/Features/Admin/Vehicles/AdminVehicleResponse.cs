using AutoHelper.Domain.Vehicles;

namespace AutoHelper.Application.Features.Admin.Vehicles;

public sealed record AdminVehicleResponse(
    Guid Id,
    string Vin,
    string Brand,
    string Model,
    int Year,
    string? Color,
    int Mileage,
    string Status,
    string? PartnerName,
    string? DocumentUrl,
    Guid OwnerId)
{
    public static AdminVehicleResponse FromVehicle(Vehicle vehicle) => new(
        Id: vehicle.Id,
        Vin: vehicle.Vin,
        Brand: vehicle.Brand,
        Model: vehicle.Model,
        Year: vehicle.Year,
        Color: vehicle.Color,
        Mileage: vehicle.Mileage,
        Status: vehicle.Status.ToString(),
        PartnerName: vehicle.PartnerName,
        DocumentUrl: vehicle.DocumentUrl,
        OwnerId: vehicle.OwnerId);
}

public sealed record AdminVehicleListResponse(
    IReadOnlyList<AdminVehicleResponse> Items,
    int TotalCount,
    int Page,
    int PageSize);
