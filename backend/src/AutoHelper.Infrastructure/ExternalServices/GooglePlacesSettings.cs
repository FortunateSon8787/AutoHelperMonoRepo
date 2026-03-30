namespace AutoHelper.Infrastructure.ExternalServices;

public sealed class GooglePlacesSettings
{
    public const string SectionName = "GooglePlaces";

    public string ApiKey { get; init; } = string.Empty;

    /// <summary>Default search radius in meters (configurable via appsettings).</summary>
    public int DefaultRadiusMeters { get; init; } = 5000;
}
