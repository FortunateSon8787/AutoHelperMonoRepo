using AutoHelper.Domain.Chats;

namespace AutoHelper.Application.Features.Chats.SendMessage;

/// <param name="AssistantReply">The text returned by the LLM.</param>
/// <param name="WasValid">True when the message was on-topic and processed normally.</param>
/// <param name="ResponseStage">
/// FaultHelp diagnostics stage: "follow_up" | "diagnostic_result" | null for other modes.
/// </param>
/// <param name="ChatStatus">Current status of the chat session after processing.</param>
/// <param name="DiagnosticResultJson">
/// Serialized <see cref="AutoHelper.Application.Features.Chats.Orchestration.DiagnosticsLlmResult"/> JSON.
/// Non-null only when ResponseStage == "diagnostic_result".
/// </param>
public sealed record SendMessageResponse(
    string AssistantReply,
    bool WasValid,
    string? ResponseStage,
    ChatStatus ChatStatus,
    string? DiagnosticResultJson = null);
