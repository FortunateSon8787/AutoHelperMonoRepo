using AutoHelper.Domain.Exceptions;

namespace AutoHelper.Domain.Partners;

/// <summary>
/// Geographic coordinates (latitude / longitude).
/// </summary>
public sealed record GeoPoint
{
    public double Lat { get; }
    public double Lng { get; }

    private GeoPoint(double lat, double lng)
    {
        Lat = lat;
        Lng = lng;
    }

    /// <summary>
    /// Creates a validated <see cref="GeoPoint"/>.
    /// </summary>
    /// <exception cref="DomainException">Thrown when coordinates are out of valid range.</exception>
    public static GeoPoint Create(double lat, double lng)
    {
        if (lat < -90 || lat > 90)
            throw new DomainException($"Latitude must be between -90 and 90. Got: {lat}.");

        if (lng < -180 || lng > 180)
            throw new DomainException($"Longitude must be between -180 and 180. Got: {lng}.");

        return new GeoPoint(lat, lng);
    }
}
