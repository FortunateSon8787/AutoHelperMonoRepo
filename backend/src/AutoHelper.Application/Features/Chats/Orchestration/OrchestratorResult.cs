namespace AutoHelper.Application.Features.Chats.Orchestration;

/// <summary>Final output of the AutoAssistantOrchestrator pipeline.</summary>
public sealed record OrchestratorResult(
    string AssistantReply,
    bool WasValid,
    bool QuotaDecremented);
