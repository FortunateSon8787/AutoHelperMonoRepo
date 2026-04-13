using System.Text.Json.Serialization;
using AutoHelper.Application.Common;
using AutoHelper.Domain.Chats;

namespace AutoHelper.Application.Features.Chats.Orchestration;

/// <summary>
/// Structured output returned by RequestClassifier (router/nano model).
/// The JSON schema of this record is passed to the LLM via Structured Outputs,
/// guaranteeing that all fields are present and correctly typed.
/// </summary>
public sealed record ClassificationResult
{
    /// <summary>
    /// Routed chat mode. Must match the chat's declared mode; if it differs
    /// the request is treated as out_of_scope.
    /// </summary>
    [JsonPropertyName("mode")]
    public string Mode { get; init; } = string.Empty;

    /// <summary>True when the user request is coherent and on-topic for the declared mode.</summary>
    [JsonPropertyName("is_valid")]
    public bool IsValid { get; init; }

    /// <summary>
    /// Reason code when is_valid = false.
    /// Possible values: off_topic | missing_context | unsafe | out_of_scope.
    /// Null when is_valid = true.
    /// </summary>
    [JsonPropertyName("rejection_reason")]
    [JsonSchemaMaxLength(64)]
    public string? RejectionReason { get; init; }

    /// <summary>
    /// True when the query is complex enough to warrant the escalation model.
    /// Ignored when is_valid = false.
    /// </summary>
    [JsonPropertyName("should_escalate")]
    public bool ShouldEscalate { get; init; }

    /// <summary>
    /// True when the quota counter should be decremented after a successful response.
    /// PartnerAdvice mode never decrements quota.
    /// </summary>
    [JsonPropertyName("should_decrement_quota")]
    public bool ShouldDecrementQuota { get; init; }

    /// <summary>
    /// Returns a pre-approved classification for follow-up answers.
    /// Used when the chat is in AwaitingUserAnswers state — any reply is valid
    /// by definition because the user is answering the assistant's own question.
    /// </summary>
    public static ClassificationResult ValidFollowUpAnswer(ChatMode mode) => new()
    {
        Mode = mode.ToString(),
        IsValid = true,
        ShouldEscalate = false,
        ShouldDecrementQuota = mode != ChatMode.PartnerAdvice,
    };
}
