using System.Text.Json.Serialization;

namespace AutoHelper.Application.Features.Chats.Orchestration;

/// <summary>
/// Structured output returned by the LLM for FaultHelp (Mode 1) diagnostics.
/// The LLM either asks follow-up questions (response_stage = follow_up)
/// or delivers the final structured diagnosis (response_stage = diagnostic_result).
/// </summary>
public sealed record DiagnosticsLlmResult
{
    /// <summary>
    /// Stage of this response.
    /// Values: "follow_up" | "diagnostic_result"
    /// </summary>
    [JsonPropertyName("response_stage")]
    public string ResponseStage { get; init; } = string.Empty;

    // ─── Follow-up stage ──────────────────────────────────────────────────────

    /// <summary>
    /// One clarifying question to ask the user.
    /// Present only when response_stage = "follow_up".
    /// </summary>
    [JsonPropertyName("follow_up_question")]
    public string? FollowUpQuestion { get; init; }

    // ─── Diagnostic result stage ──────────────────────────────────────────────

    /// <summary>Potential problems identified, ordered by probability descending.</summary>
    [JsonPropertyName("potential_problems")]
    public DiagnosticProblem[]? PotentialProblems { get; init; }

    /// <summary>
    /// Urgency level.
    /// Values: "low" | "medium" | "high" | "stop_driving"
    /// </summary>
    [JsonPropertyName("urgency")]
    public string? Urgency { get; init; }

    /// <summary>Current risks if the issue is left unaddressed.</summary>
    [JsonPropertyName("current_risks")]
    public string? CurrentRisks { get; init; }

    /// <summary>Whether it is safe to continue driving (true/false). Null for follow-up stage.</summary>
    [JsonPropertyName("safe_to_drive")]
    public bool? SafeToDrive { get; init; }

    /// <summary>
    /// Suggested partner service category and urgency (e.g. "car_service:high").
    /// Null when partner suggestions are disabled in admin config or not applicable.
    /// </summary>
    [JsonPropertyName("suggested_partner_category")]
    public string? SuggestedPartnerCategory { get; init; }

    /// <summary>Mandatory disclaimer about the estimate nature of the response.</summary>
    [JsonPropertyName("disclaimer")]
    public string? Disclaimer { get; init; }
}

/// <summary>A single potential problem identified during diagnostics.</summary>
public sealed record DiagnosticProblem
{
    /// <summary>Short name/title of the problem.</summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>Probability estimate in range 0–1.</summary>
    [JsonPropertyName("probability")]
    public double Probability { get; init; }

    /// <summary>Possible root causes.</summary>
    [JsonPropertyName("possible_causes")]
    public string? PossibleCauses { get; init; }

    /// <summary>Recommended actions the user should take.</summary>
    [JsonPropertyName("recommended_actions")]
    public string? RecommendedActions { get; init; }
}
