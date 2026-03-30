namespace AutoHelper.Application.Common.Interfaces;

/// <summary>
/// Aggregates own registered partners (from DB) and external Google Places results
/// into a unified ranked list of partner cards for the AI advisor.
/// </summary>
public interface IPartnerSearchService
{
    /// <summary>
    /// Returns up to <paramref name="maxResults"/> partner cards, with own partners always first.
    /// If own partners count &lt; maxResults, fills the rest from Google Places API.
    /// </summary>
    Task<IReadOnlyList<PartnerCard>> FindPartnersAsync(
        double lat,
        double lng,
        string serviceCategory,
        string languageCode,
        int maxResults,
        CancellationToken ct);
}

/// <summary>
/// Unified partner card passed to the LLM for response generation.
/// Covers both own partners (from DB) and Google Places results.
/// </summary>
public sealed record PartnerCard(
    string Source,               // "own_partner" | "google_places"
    bool IsPriorityPartner,
    string Name,
    string? Address,
    string? Phone,
    string? Website,
    double? Rating,
    int? ReviewsCount,
    bool? IsOpenNow,
    double DistanceKm,
    string? Services,
    bool HasWarning);            // IsPotentiallyUnfit for own partners
