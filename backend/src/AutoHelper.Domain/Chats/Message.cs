using AutoHelper.Domain.Common;

namespace AutoHelper.Domain.Chats;

/// <summary>
/// A single message exchanged in a chat session.
/// IsValid = false marks messages rejected by the topic guard
/// (does NOT consume subscription quota).
/// </summary>
public sealed class Message : Entity<Guid>
{
    public Guid ChatId { get; private set; }
    public MessageRole Role { get; private set; }
    public string Content { get; private set; } = string.Empty;

    /// <summary>
    /// False when the user's request was off-topic or invalid.
    /// Invalid messages are stored for audit but do not decrement the quota.
    /// </summary>
    public bool IsValid { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // ─── EF Core ──────────────────────────────────────────────────────────────

    private Message() { }

    // ─── Factory methods ──────────────────────────────────────────────────────

    internal static Message CreateUserMessage(Guid chatId, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Message content cannot be empty.", nameof(content));

        return new Message
        {
            Id = Guid.NewGuid(),
            ChatId = chatId,
            Role = MessageRole.User,
            Content = content.Trim(),
            IsValid = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    internal static Message CreateAssistantMessage(Guid chatId, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Assistant response content cannot be empty.", nameof(content));

        return new Message
        {
            Id = Guid.NewGuid(),
            ChatId = chatId,
            Role = MessageRole.Assistant,
            Content = content.Trim(),
            IsValid = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    internal static Message CreateInvalidUserMessage(Guid chatId, string content)
    {
        return new Message
        {
            Id = Guid.NewGuid(),
            ChatId = chatId,
            Role = MessageRole.User,
            Content = string.IsNullOrWhiteSpace(content) ? string.Empty : content.Trim(),
            IsValid = false,
            CreatedAt = DateTime.UtcNow
        };
    }
}
