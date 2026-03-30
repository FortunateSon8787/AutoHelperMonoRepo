namespace AutoHelper.Application.Common.Interfaces;

/// <summary>
/// Fetches nearby places from Google Places API (New).
/// The API key is kept exclusively on the backend — never exposed to the client.
/// </summary>
public interface IGooglePlacesService
{
    /// <summary>
    /// Searches for nearby places of the given type within the specified radius.
    /// </summary>
    /// <param name="lat">Latitude of the search origin.</param>
    /// <param name="lng">Longitude of the search origin.</param>
    /// <param name="radiusMeters">Search radius in meters.</param>
    /// <param name="placeType">Google Places included type (e.g. "car_repair", "car_wash").</param>
    /// <param name="languageCode">BCP-47 language code for localised names and addresses.</param>
    /// <param name="maxResults">Maximum number of results to return.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<IReadOnlyList<GooglePlaceResult>> SearchNearbyAsync(
        double lat,
        double lng,
        int radiusMeters,
        string placeType,
        string languageCode,
        int maxResults,
        CancellationToken ct);
}

/// <summary>Minimal place data returned from Google Places API (New).</summary>
public sealed record GooglePlaceResult(
    string PlaceId,
    string Name,
    string? Address,
    double? Lat,
    double? Lng,
    double? Rating,
    int? UserRatingsTotal,
    bool? IsOpenNow,
    string? PhoneNumber,
    string? Website,
    double DistanceMeters);
