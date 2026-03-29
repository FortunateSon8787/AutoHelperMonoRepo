using AutoHelper.Domain.Common;

namespace AutoHelper.Domain.Chats;

/// <summary>
/// Audit record for user messages rejected by the RequestClassifier.
/// Stored for analysis and abuse detection — does NOT consume subscription quota.
/// Physical deletion is prohibited (soft-delete not applicable — records are append-only).
/// </summary>
public sealed class InvalidChatRequest : Entity<Guid>
{
    public Guid ChatId { get; private set; }

    public Guid CustomerId { get; private set; }

    /// <summary>Raw user input that was rejected. Stored as-is for audit purposes.</summary>
    public string UserInput { get; private set; } = string.Empty;

    /// <summary>
    /// Rejection reason returned by the classifier.
    /// Values: off_topic | missing_context | unsafe | out_of_scope.
    /// </summary>
    public string RejectionReason { get; private set; } = string.Empty;

    public DateTime CreatedAt { get; private set; }

    // ─── EF Core ──────────────────────────────────────────────────────────────

    private InvalidChatRequest() { }

    // ─── Factory ──────────────────────────────────────────────────────────────

    public static InvalidChatRequest Create(
        Guid chatId,
        Guid customerId,
        string userInput,
        string rejectionReason)
    {
        return new InvalidChatRequest
        {
            Id = Guid.NewGuid(),
            ChatId = chatId,
            CustomerId = customerId,
            UserInput = userInput,
            RejectionReason = string.IsNullOrWhiteSpace(rejectionReason) ? "unknown" : rejectionReason,
            CreatedAt = DateTime.UtcNow
        };
    }
}
