namespace AutoHelper.Application.Features.Chats.CreateChat;

/// <summary>
/// Returned after a chat session is created.
/// For FaultHelp mode the assistant already processes the initial diagnostics input,
/// so the first reply (follow-up questions or a direct result) is included here.
/// For WorkClarification mode the structured assessment JSON is included here.
/// </summary>
public sealed record CreateChatResponse(
    Guid ChatId,
    string? InitialAssistantReply,
    string? DiagnosticResultJson = null,
    string? WorkClarificationResultJson = null);
