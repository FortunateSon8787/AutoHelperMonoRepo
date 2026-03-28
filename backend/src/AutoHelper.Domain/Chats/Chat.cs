using AutoHelper.Domain.Common;
using AutoHelper.Domain.Exceptions;

namespace AutoHelper.Domain.Chats;

/// <summary>
/// Aggregate root for an AI assistant chat session.
/// A chat belongs to a customer and may optionally be linked to a specific vehicle.
/// </summary>
public sealed class Chat : AggregateRoot<Guid>
{
    private readonly List<Message> _messages = [];

    public Guid CustomerId { get; private set; }

    /// <summary>Optional vehicle context for the conversation.</summary>
    public Guid? VehicleId { get; private set; }

    public ChatMode Mode { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    public IReadOnlyCollection<Message> Messages => _messages.AsReadOnly();

    // ─── EF Core ──────────────────────────────────────────────────────────────

    private Chat() { }

    // ─── Factory method ───────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new chat session for the given customer.
    /// </summary>
    public static Chat Create(
        Guid customerId,
        ChatMode mode,
        string title,
        Guid? vehicleId = null)
    {
        if (customerId == Guid.Empty)
            throw new DomainException("CustomerId cannot be empty.");

        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Chat title cannot be empty.");

        return new Chat
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Mode = mode,
            Title = title.Trim(),
            VehicleId = vehicleId,
            CreatedAt = DateTime.UtcNow
        };
    }

    // ─── Business operations ──────────────────────────────────────────────────

    /// <summary>
    /// Records the user's message and the LLM's response as a valid exchange.
    /// </summary>
    public void AddExchange(string userContent, string assistantContent)
    {
        _messages.Add(Message.CreateUserMessage(Id, userContent));
        _messages.Add(Message.CreateAssistantMessage(Id, assistantContent));
    }

    /// <summary>
    /// Records an off-topic or rejected user message.
    /// Does NOT produce an assistant reply entry — the caller returns a fixed rejection text.
    /// </summary>
    public void AddInvalidUserMessage(string userContent)
    {
        _messages.Add(Message.CreateInvalidUserMessage(Id, userContent));
    }
}
