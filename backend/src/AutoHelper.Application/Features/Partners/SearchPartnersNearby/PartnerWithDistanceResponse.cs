namespace AutoHelper.Application.Features.Partners.SearchPartnersNearby;

/// <summary>
/// Partner profile enriched with the calculated distance from the search origin.
/// </summary>
public sealed record PartnerWithDistanceResponse(
    Guid Id,
    string Name,
    string Type,
    string Specialization,
    string Description,
    string Address,
    double LocationLat,
    double LocationLng,
    double DistanceKm,
    string WorkingOpenFrom,
    string WorkingOpenTo,
    string WorkingDays,
    bool IsOpenNow,
    string ContactsPhone,
    string? ContactsWebsite,
    string? LogoUrl,
    bool IsVerified,
    double AverageRating,
    int ReviewsCount);
