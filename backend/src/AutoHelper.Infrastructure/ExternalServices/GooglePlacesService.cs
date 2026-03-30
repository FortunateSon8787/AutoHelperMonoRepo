using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AutoHelper.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AutoHelper.Infrastructure.ExternalServices;

/// <summary>
/// Google Places API (New) client.
/// Uses Nearby Search endpoint: POST https://places.googleapis.com/v1/places:searchNearby
/// API key is kept server-side only.
/// </summary>
public sealed class GooglePlacesService(
    HttpClient httpClient,
    IOptions<GooglePlacesSettings> options,
    ILogger<GooglePlacesService> logger) : IGooglePlacesService
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private const string NearbySearchUrl = "https://places.googleapis.com/v1/places:searchNearby";

    public async Task<IReadOnlyList<GooglePlaceResult>> SearchNearbyAsync(
        double lat,
        double lng,
        int radiusMeters,
        string placeType,
        string languageCode,
        int maxResults,
        CancellationToken ct)
    {
        var requestBody = new
        {
            includedTypes = new[] { placeType },
            maxResultCount = Math.Clamp(maxResults, 1, 20),
            languageCode,
            locationRestriction = new
            {
                circle = new
                {
                    center = new { latitude = lat, longitude = lng },
                    radius = (double)radiusMeters
                }
            }
        };

        var json = JsonSerializer.Serialize(requestBody, JsonOpts);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, NearbySearchUrl);
        requestMessage.Content = content;
        requestMessage.Headers.Add("X-Goog-Api-Key", options.Value.ApiKey);
        requestMessage.Headers.Add("X-Goog-FieldMask",
            "places.id,places.displayName,places.formattedAddress," +
            "places.location,places.rating,places.userRatingCount," +
            "places.currentOpeningHours.openNow,places.nationalPhoneNumber,places.websiteUri");

        try
        {
            using var response = await httpClient.SendAsync(requestMessage, ct);
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadFromJsonAsync<NearbySearchResponse>(JsonOpts, ct);
            if (body?.Places is null or { Length: 0 })
                return [];

            return body.Places
                .Select(p => MapToResult(p, lat, lng))
                .ToList();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "Google Places Nearby Search failed for lat={Lat} lng={Lng} type={Type}",
                lat, lng, placeType);
            return [];
        }
    }

    // ─── Mapping ──────────────────────────────────────────────────────────────

    private static GooglePlaceResult MapToResult(PlaceDto place, double originLat, double originLng)
    {
        var placeLat = place.Location?.Latitude;
        var placeLng = place.Location?.Longitude;

        var distanceMeters = placeLat.HasValue && placeLng.HasValue
            ? HaversineMeters(originLat, originLng, placeLat.Value, placeLng.Value)
            : 0.0;

        return new GooglePlaceResult(
            PlaceId: place.Id ?? string.Empty,
            Name: place.DisplayName?.Text ?? string.Empty,
            Address: place.FormattedAddress,
            Lat: placeLat,
            Lng: placeLng,
            Rating: place.Rating,
            UserRatingsTotal: place.UserRatingCount,
            IsOpenNow: place.CurrentOpeningHours?.OpenNow,
            PhoneNumber: place.NationalPhoneNumber,
            Website: place.WebsiteUri,
            DistanceMeters: distanceMeters);
    }

    private static double HaversineMeters(double lat1, double lng1, double lat2, double lng2)
    {
        const double earthRadiusM = 6_371_000.0;
        var dLat = (lat2 - lat1) * Math.PI / 180.0;
        var dLng = (lng2 - lng1) * Math.PI / 180.0;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
              + Math.Cos(lat1 * Math.PI / 180.0) * Math.Cos(lat2 * Math.PI / 180.0)
              * Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
        return earthRadiusM * 2 * Math.Asin(Math.Sqrt(a));
    }

    // ─── Response DTOs (internal) ─────────────────────────────────────────────

    private sealed class NearbySearchResponse
    {
        [JsonPropertyName("places")]
        public PlaceDto[]? Places { get; init; }
    }

    private sealed class PlaceDto
    {
        [JsonPropertyName("id")]
        public string? Id { get; init; }

        [JsonPropertyName("displayName")]
        public LocalizedTextDto? DisplayName { get; init; }

        [JsonPropertyName("formattedAddress")]
        public string? FormattedAddress { get; init; }

        [JsonPropertyName("location")]
        public LatLngDto? Location { get; init; }

        [JsonPropertyName("rating")]
        public double? Rating { get; init; }

        [JsonPropertyName("userRatingCount")]
        public int? UserRatingCount { get; init; }

        [JsonPropertyName("currentOpeningHours")]
        public OpeningHoursDto? CurrentOpeningHours { get; init; }

        [JsonPropertyName("nationalPhoneNumber")]
        public string? NationalPhoneNumber { get; init; }

        [JsonPropertyName("websiteUri")]
        public string? WebsiteUri { get; init; }
    }

    private sealed class LocalizedTextDto
    {
        [JsonPropertyName("text")]
        public string? Text { get; init; }
    }

    private sealed class LatLngDto
    {
        [JsonPropertyName("latitude")]
        public double? Latitude { get; init; }

        [JsonPropertyName("longitude")]
        public double? Longitude { get; init; }
    }

    private sealed class OpeningHoursDto
    {
        [JsonPropertyName("openNow")]
        public bool? OpenNow { get; init; }
    }
}
