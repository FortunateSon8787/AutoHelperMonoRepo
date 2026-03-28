namespace AutoHelper.Application.Features.Chats;

/// <summary>
/// Canonical error strings for chat handlers.
/// Referenced in both handlers and endpoint routing to prevent string-matching drift.
/// </summary>
public static class ChatErrors
{
    public const string NotAuthenticated = "User is not authenticated.";
    public const string CustomerNotFound = "Customer not found.";
    public const string ChatNotFound = "Chat not found.";
    public const string SubscriptionRequired = "Active Premium subscription is required to use this chat mode.";
    public const string CreateSubscriptionRequired = "AI chat requires an active Premium subscription.";
}
