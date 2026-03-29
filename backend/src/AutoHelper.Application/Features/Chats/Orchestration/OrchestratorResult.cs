using AutoHelper.Domain.Chats;

namespace AutoHelper.Application.Features.Chats.Orchestration;

/// <summary>Final output of the AutoAssistantOrchestrator pipeline.</summary>
public sealed record OrchestratorResult(
    string AssistantReply,
    bool WasValid,
    bool QuotaDecremented,
    /// <summary>
    /// Diagnostics response stage: "follow_up" | "diagnostic_result" | null (non-FaultHelp modes).
    /// </summary>
    string? ResponseStage,
    ChatStatus ChatStatus);
