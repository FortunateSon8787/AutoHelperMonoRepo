using AutoHelper.Domain.Common;

namespace AutoHelper.Domain.Vehicles;

/// <summary>
/// Aggregate root representing a vehicle identified by its unique VIN.
/// A vehicle is registered once and ownership can change over time.
/// </summary>
public sealed class Vehicle : AggregateRoot<Guid>
{
    /// <summary>Vehicle Identification Number — globally unique in the system.</summary>
    public string Vin { get; private set; } = string.Empty;

    public string Brand { get; private set; } = string.Empty;

    public string Model { get; private set; } = string.Empty;

    public int Year { get; private set; }

    public string? Color { get; private set; }

    public int Mileage { get; private set; }

    public VehicleStatus Status { get; private set; }

    /// <summary>FK to the current owner (Customer).</summary>
    public Guid OwnerId { get; private set; }

    // ─── EF Core ──────────────────────────────────────────────────────────────

    private Vehicle() { }

    // ─── Factory method ───────────────────────────────────────────────────────

    public static Vehicle Create(
        string vin,
        string brand,
        string model,
        int year,
        Guid ownerId,
        string? color = null,
        int mileage = 0)
    {
        return new Vehicle
        {
            Id = Guid.NewGuid(),
            Vin = vin.Trim().ToUpperInvariant(),
            Brand = brand.Trim(),
            Model = model.Trim(),
            Year = year,
            Color = color?.Trim(),
            Mileage = mileage,
            Status = VehicleStatus.Active,
            OwnerId = ownerId
        };
    }
}
