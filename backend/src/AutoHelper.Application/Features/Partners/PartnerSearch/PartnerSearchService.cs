using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Domain.Partners;
using Microsoft.Extensions.Logging;

namespace AutoHelper.Application.Features.Partners.PartnerSearch;

/// <summary>
/// Implements the partner aggregation logic described in AUT-21:
///
///   1. Load own active+verified partners from DB by type + geo radius.
///   2. If own partners >= maxResults — return only own partners (no Google call).
///   3. If own partners < maxResults — fetch the missing count from Google Places API.
///   4. Own partners are always listed first.
/// </summary>
public sealed class PartnerSearchService(
    IPartnerRepository partnerRepository,
    IGooglePlacesService googlePlaces,
    ILogger<PartnerSearchService> logger) : IPartnerSearchService
{
    private const double EarthRadiusKm = 6371.0;

    public async Task<IReadOnlyList<PartnerCard>> FindPartnersAsync(
        double lat,
        double lng,
        string serviceCategory,
        string languageCode,
        int maxResults,
        CancellationToken ct)
    {
        // ── Step 1: own partners from DB ──────────────────────────────────────
        var ownCards = new List<PartnerCard>();

        if (PartnerCategoryMapper.TryGetPartnerType(serviceCategory, out var partnerType))
        {
            var ownPartners = await partnerRepository.SearchByTypeAndLocationAsync(
                partnerType, lat, lng, radiusKm: double.MaxValue, ct);

            ownCards = ownPartners
                .Select(p => ToOwnCard(p, lat, lng))
                .OrderBy(c => c.DistanceKm)
                .Take(maxResults)
                .ToList();
        }

        // ── Step 2: short-circuit if own partners fill the quota ──────────────
        if (ownCards.Count >= maxResults)
            return ownCards;

        // ── Step 3: fill remainder from Google Places ─────────────────────────
        var needed = maxResults - ownCards.Count;
        var googleType = PartnerCategoryMapper.GetGooglePlaceType(serviceCategory);

        logger.LogInformation("Google type is: {GoogleType}", googleType);
        var googleResults = await googlePlaces.SearchNearbyAsync(
            lat, lng,
            radiusMeters: (int)(50_000), // pass a broad radius; own-partner radius is handled by DB
            placeType: googleType,
            languageCode: languageCode,
            maxResults: needed * 3, // over-fetch to allow deduplication
            ct);

        // Deduplicate: skip Google results whose names match an own partner
        var ownNames = new HashSet<string>(
            ownCards.Select(c => c.Name),
            StringComparer.OrdinalIgnoreCase);

        var googleCards = googleResults
            .Where(g => !ownNames.Contains(g.Name))
            .Take(needed)
            .Select(ToGoogleCard)
            .ToList();

        // ── Step 4: open + nearest first ─────────────────────────────────────
        return [.. ownCards.Concat(googleCards)
            .OrderByDescending(c => c.IsOpenNow == true)
            .ThenBy(c => c.IsOpenNow == false)
            .ThenBy(c => c.DistanceKm)];
    }

    // ─── Mapping ──────────────────────────────────────────────────────────────

    private static PartnerCard ToOwnCard(Partner p, double originLat, double originLng)
    {
        var distanceKm = HaversineKm(originLat, originLng, p.Location.Lat, p.Location.Lng);
        var currentTime = TimeOnly.FromDateTime(DateTime.UtcNow);
        var isOpenNow = currentTime >= p.WorkingHours.OpenFrom && currentTime <= p.WorkingHours.OpenTo;

        return new PartnerCard(
            Source: "own_partner",
            IsPriorityPartner: true,
            Name: p.Name,
            Address: p.Address,
            Phone: p.Contacts.Phone,
            Website: p.Contacts.Website,
            Rating: null,           // own partner ratings computed separately if needed
            ReviewsCount: null,
            IsOpenNow: isOpenNow,
            DistanceKm: Math.Round(distanceKm, 2),
            Services: p.Specialization,
            HasWarning: p.IsPotentiallyUnfit);
    }

    private static PartnerCard ToGoogleCard(GooglePlaceResult g) =>
        new(
            Source: "google_places",
            IsPriorityPartner: false,
            Name: g.Name,
            Address: g.Address,
            Phone: g.PhoneNumber,
            Website: g.Website,
            Rating: g.Rating,
            ReviewsCount: g.UserRatingsTotal,
            IsOpenNow: g.IsOpenNow,
            DistanceKm: Math.Round(g.DistanceMeters / 1000.0, 2),
            Services: null,
            HasWarning: false);

    private static double HaversineKm(double lat1, double lng1, double lat2, double lng2)
    {
        var dLat = (lat2 - lat1) * Math.PI / 180.0;
        var dLng = (lng2 - lng1) * Math.PI / 180.0;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
              + Math.Cos(lat1 * Math.PI / 180.0) * Math.Cos(lat2 * Math.PI / 180.0)
              * Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
        return EarthRadiusKm * 2 * Math.Asin(Math.Sqrt(a));
    }
}
