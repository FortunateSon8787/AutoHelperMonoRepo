using System.Text.Json.Serialization;

namespace AutoHelper.Application.Features.Chats.Orchestration;

/// <summary>
/// Structured output returned by the LLM for WorkClarification (Mode 2).
/// The bot evaluates the reasonableness of the work performed and its pricing
/// based on market benchmarks, then gives an overall honesty assessment.
/// </summary>
public sealed record WorkClarificationLlmResult
{
    /// <summary>
    /// Relevance of the stated reason for performing the work.
    /// Values: "low" | "medium" | "high" | "unclear"
    /// </summary>
    [JsonPropertyName("work_reason_relevance")]
    public string WorkReasonRelevance { get; init; } = string.Empty;

    /// <summary>Explanation of the work reason assessment in plain language.</summary>
    [JsonPropertyName("work_reason_explanation")]
    public string WorkReasonExplanation { get; init; } = string.Empty;

    /// <summary>
    /// Comparison of labor cost against market benchmarks.
    /// Values: "below_market" | "near_market" | "above_market" | "unknown"
    /// </summary>
    [JsonPropertyName("labor_price_assessment")]
    public string LaborPriceAssessment { get; init; } = string.Empty;

    /// <summary>Explanation of the labor pricing assessment in plain language.</summary>
    [JsonPropertyName("labor_price_explanation")]
    public string LaborPriceExplanation { get; init; } = string.Empty;

    /// <summary>
    /// Comparison of parts cost against market benchmarks.
    /// Values: "below_market" | "near_market" | "above_market" | "unknown"
    /// </summary>
    [JsonPropertyName("parts_price_assessment")]
    public string PartsPriceAssessment { get; init; } = string.Empty;

    /// <summary>Explanation of the parts pricing assessment in plain language.</summary>
    [JsonPropertyName("parts_price_explanation")]
    public string PartsPriceExplanation { get; init; } = string.Empty;

    /// <summary>
    /// Strength of guarantees given by the service center.
    /// Values: "weak" | "normal" | "strong" | "unclear"
    /// </summary>
    [JsonPropertyName("guarantee_assessment")]
    public string GuaranteeAssessment { get; init; } = string.Empty;

    /// <summary>Explanation of the guarantee assessment in plain language.</summary>
    [JsonPropertyName("guarantee_explanation")]
    public string GuaranteeExplanation { get; init; } = string.Empty;

    /// <summary>
    /// Overall honesty rating for the service center.
    /// Values: "poor" | "mixed" | "fair" | "good" | "unknown"
    /// </summary>
    [JsonPropertyName("overall_honesty")]
    public string OverallHonesty { get; init; } = string.Empty;

    /// <summary>Summary explanation of the overall honesty rating.</summary>
    [JsonPropertyName("overall_explanation")]
    public string OverallExplanation { get; init; } = string.Empty;

    /// <summary>What the customer should expect from subsequent servicing at this center.</summary>
    [JsonPropertyName("future_expectations")]
    public string? FutureExpectations { get; init; }

    /// <summary>Recommended mileage interval before repeating the work or replacing parts.</summary>
    [JsonPropertyName("repeat_interval_km")]
    public int? RepeatIntervalKm { get; init; }

    /// <summary>Mandatory disclaimer about the estimate nature of the assessment.</summary>
    [JsonPropertyName("disclaimer")]
    public string? Disclaimer { get; init; }
}
