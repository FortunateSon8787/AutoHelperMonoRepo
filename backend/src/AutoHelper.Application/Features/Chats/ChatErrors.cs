namespace AutoHelper.Application.Features.Chats;

/// <summary>
/// Aliases to AppErrors.Chat for backwards compatibility within the Chats feature.
/// All canonical error definitions live in AppErrors.Chat.
/// </summary>
[Obsolete("Use AppErrors.Chat directly instead of ChatErrors.")]
public static class ChatErrors
{
    public const string NotAuthenticated = "AUTH_001: User is not authenticated.";
    public const string CustomerNotFound = "CHAT_001: Customer not found.";
    public const string ChatNotFound = "CHAT_002: Chat not found.";
    public const string SubscriptionRequired = "CHAT_003: Active subscription is required to use this chat mode.";
    public const string CreateSubscriptionRequired = "CHAT_004: AI chat requires an active subscription.";
    public const string DiagnosticsInputRequired = "CHAT_005: DiagnosticsInput is required for FaultHelp mode.";
    public const string WorkClarificationInputRequired = "CHAT_006: WorkClarificationInput is required for WorkClarification mode.";
    public const string PartnerAdviceInputRequired = "CHAT_007: PartnerAdviceInput is required for PartnerAdvice mode.";
    public const string ChatIsCompleted = "CHAT_008: This chat is completed and no longer accepts messages.";
}
