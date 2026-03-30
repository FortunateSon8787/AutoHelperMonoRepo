using System.Text.Json.Serialization;

namespace AutoHelper.Application.Features.Chats.Orchestration;

/// <summary>
/// Structured output returned by the LLM in Step 1 of the PartnerAdvice pipeline.
/// The LLM determines the service category and urgency from the user's request,
/// then formats the final text response based on partner cards prepared by the backend.
/// </summary>
public sealed record PartnerAdviceLlmResult
{
    /// <summary>
    /// Service category determined from the user request.
    /// Allowed values: tow_truck | tire_service | car_service | car_wash | electrician | auto_service | other
    /// </summary>
    [JsonPropertyName("service_category")]
    public string? ServiceCategory { get; init; }

    /// <summary>Urgency level: low | medium | high</summary>
    [JsonPropertyName("urgency")]
    public string? Urgency { get; init; }

    /// <summary>The final user-facing response text, formatted using the partner cards.</summary>
    [JsonPropertyName("response_text")]
    public string? ResponseText { get; init; }
}
