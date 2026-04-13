using AutoHelper.Domain.Chats;

namespace AutoHelper.Application.Features.Chats.Orchestration;

/// <summary>Final output of the AutoAssistantOrchestrator pipeline.</summary>
public sealed record OrchestratorResult(
    string AssistantReply,
    bool WasValid,
    bool QuotaDecremented,
    /// <summary>
    /// Diagnostics response stage: "follow_up" | "diagnostic_result" | "work_clarification_result" | "partner_advice_result" | null.
    /// </summary>
    string? ResponseStage,
    ChatStatus ChatStatus,
    /// <summary>
    /// Serialized <see cref="DiagnosticsLlmResult"/> JSON.
    /// Non-null only when ResponseStage == "diagnostic_result".
    /// </summary>
    string? DiagnosticResultJson = null,
    /// <summary>
    /// Serialized <see cref="WorkClarificationLlmResult"/> JSON.
    /// Non-null only when ResponseStage == "work_clarification_result".
    /// </summary>
    string? WorkClarificationResultJson = null,
    /// <summary>
    /// Serialized <see cref="PartnerAdviceLlmResult"/> JSON (partners list + summary).
    /// Non-null only when ResponseStage == "partner_advice_result".
    /// </summary>
    string? PartnerAdviceResultJson = null);
