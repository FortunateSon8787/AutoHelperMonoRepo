using AutoHelper.Domain.Vehicles;

namespace AutoHelper.Application.Common.Interfaces;

/// <summary>
/// Provides data access operations for the Vehicle aggregate.
/// </summary>
public interface IVehicleRepository
{
    /// <summary>Finds a vehicle by its VIN (case-insensitive).</summary>
    Task<Vehicle?> GetByVinAsync(string vin, CancellationToken ct);

    /// <summary>Checks whether a vehicle with the given VIN already exists.</summary>
    Task<bool> ExistsByVinAsync(string vin, CancellationToken ct);

    /// <summary>Adds a new vehicle to the repository (tracked, not yet persisted).</summary>
    void Add(Vehicle vehicle);
}
