using System.Text.Json.Serialization;

namespace AutoHelper.Application.Features.Chats.Orchestration;

/// <summary>
/// Structured output returned by the LLM in the PartnerAdvice pipeline.
///
/// Step 1 (classifier): fills ServiceCategory + Urgency only.
/// Step 2 (formatter): fills Partners list + optional Summary. ServiceCategory and Urgency are null.
/// </summary>
public sealed record PartnerAdviceLlmResult
{
    /// <summary>
    /// Service category determined from the user request (Step 1 only).
    /// Allowed values: tow_truck | tire_service | car_service | car_wash | electrician | auto_service | other
    /// </summary>
    [JsonPropertyName("service_category")]
    public string? ServiceCategory { get; init; }

    /// <summary>Urgency level determined in Step 1: low | medium | high</summary>
    [JsonPropertyName("urgency")]
    public string? Urgency { get; init; }

    /// <summary>
    /// Structured list of partner entries (Step 2 only).
    /// Each entry corresponds to one partner card from PARTNER_CARDS.
    /// </summary>
    [JsonPropertyName("partners")]
    public List<PartnerAdviceEntry>? Partners { get; init; }

    /// <summary>
    /// Optional short summary / advice text shown above the partner list (Step 2 only).
    /// May include urgency advice or general guidance.
    /// </summary>
    [JsonPropertyName("summary")]
    public string? Summary { get; init; }
}

/// <summary>One partner entry in the structured PartnerAdvice response.</summary>
public sealed record PartnerAdviceEntry
{
    /// <summary>Partner display name.</summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>Street address or area description.</summary>
    [JsonPropertyName("address")]
    public string? Address { get; init; }

    /// <summary>Distance from the user in kilometres.</summary>
    [JsonPropertyName("distance_km")]
    public double DistanceKm { get; init; }

    /// <summary>Average rating (1-5). Null if not available.</summary>
    [JsonPropertyName("rating")]
    public double? Rating { get; init; }

    /// <summary>Total number of reviews. Null if not available.</summary>
    [JsonPropertyName("reviews_count")]
    public int? ReviewsCount { get; init; }

    /// <summary>True if the partner is currently open.</summary>
    [JsonPropertyName("is_open_now")]
    public bool? IsOpenNow { get; init; }

    /// <summary>Phone number. Null if not available.</summary>
    [JsonPropertyName("phone")]
    public string? Phone { get; init; }

    /// <summary>Website URL. Null if not available.</summary>
    [JsonPropertyName("website")]
    public string? Website { get; init; }

    /// <summary>Services offered (comma-separated string). Null if not specified.</summary>
    [JsonPropertyName("services")]
    public string? Services { get; init; }

    /// <summary>True if this is a verified own partner (priority listing).</summary>
    [JsonPropertyName("is_priority")]
    public bool IsPriority { get; init; }

    /// <summary>True if the partner has a warning flag (e.g. potentially unfit).</summary>
    [JsonPropertyName("has_warning")]
    public bool HasWarning { get; init; }
}
