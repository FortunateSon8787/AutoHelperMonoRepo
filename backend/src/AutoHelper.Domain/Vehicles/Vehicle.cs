using System.Text.RegularExpressions;
using AutoHelper.Domain.Common;
using AutoHelper.Domain.Exceptions;

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

    // ─── VIN invariant ────────────────────────────────────────────────────────

    /// <summary>
    /// Valid VIN: exactly 17 uppercase alphanumeric chars, excluding I, O, Q
    /// (ISO 3779 standard).
    /// </summary>
    private static readonly Regex VinPattern =
        new(@"^[A-HJ-NPR-Z0-9]{17}$", RegexOptions.Compiled);

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
        var normalizedVin = vin.Trim().ToUpperInvariant();

        if (!VinPattern.IsMatch(normalizedVin))
            throw new DomainException(
                "VIN must be exactly 17 alphanumeric characters (I, O, Q are not allowed).");

        return new Vehicle
        {
            Id = Guid.NewGuid(),
            Vin = normalizedVin,
            Brand = brand.Trim(),
            Model = model.Trim(),
            Year = year,
            Color = color?.Trim(),
            Mileage = mileage,
            Status = VehicleStatus.Active,
            OwnerId = ownerId
        };
    }

    // ─── Business operations ──────────────────────────────────────────────────

    public void UpdateDetails(string brand, string model, int year, string? color, int mileage)
    {
        Brand = brand.Trim();
        Model = model.Trim();
        Year = year;
        Color = color?.Trim();
        Mileage = mileage;
    }
}
