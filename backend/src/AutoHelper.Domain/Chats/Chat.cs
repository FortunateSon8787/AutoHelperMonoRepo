using AutoHelper.Domain.Common;
using AutoHelper.Domain.Exceptions;

namespace AutoHelper.Domain.Chats;

/// <summary>
/// Aggregate root for an AI assistant chat session.
/// A chat belongs to a customer and may optionally be linked to a specific vehicle.
///
/// FaultHelp (Mode 1) state machine:
///   Active → AwaitingUserAnswers → Active → … → FinalAnswerSent → Completed
/// Other modes: Active → Completed (or remain Active indefinitely).
/// </summary>
public sealed class Chat : AggregateRoot<Guid>
{
    private readonly List<Message> _messages = [];

    public Guid CustomerId { get; private set; }

    /// <summary>Optional vehicle context for the conversation.</summary>
    public Guid? VehicleId { get; private set; }

    public ChatMode Mode { get; private set; }
    public ChatStatus Status { get; private set; } = ChatStatus.Active;
    public string Title { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// True after the final diagnostic result is sent, while one additional question is still allowed.
    /// Transitions to false (and Status → Completed) once that question is answered.
    /// </summary>
    public bool AllowOneAdditionalQuestion { get; private set; }

    public IReadOnlyCollection<Message> Messages => _messages.AsReadOnly();

    // ─── EF Core ──────────────────────────────────────────────────────────────

    private Chat() { }

    // ─── Factory method ───────────────────────────────────────────────────────

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
            CreatedAt = DateTime.UtcNow,
            Status = ChatStatus.Active
        };
    }

    // ─── Business operations ──────────────────────────────────────────────────

    /// <summary>Records the user's message and the LLM's response as a valid exchange.</summary>
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

    // ─── FaultHelp state transitions ─────────────────────────────────────────

    /// <summary>
    /// Bot asked follow-up questions; waiting for user answers.
    /// Valid only in FaultHelp mode while Active.
    /// </summary>
    public void TransitionToAwaitingAnswers()
    {
        if (Mode != ChatMode.FaultHelp)
            throw new DomainException("AwaitingUserAnswers state is only valid for FaultHelp mode.");
        if (Status != ChatStatus.Active)
            throw new DomainException($"Cannot transition to AwaitingUserAnswers from {Status}.");

        Status = ChatStatus.AwaitingUserAnswers;
    }

    /// <summary>
    /// User answered follow-up questions; resume processing.
    /// </summary>
    public void TransitionBackToActive()
    {
        if (Status != ChatStatus.AwaitingUserAnswers)
            throw new DomainException($"Cannot transition back to Active from {Status}.");

        Status = ChatStatus.Active;
    }

    /// <summary>
    /// The final diagnostic result was delivered.
    /// Exactly one additional user question is still permitted.
    /// </summary>
    public void TransitionToFinalAnswerSent()
    {
        if (Mode != ChatMode.FaultHelp)
            throw new DomainException("FinalAnswerSent state is only valid for FaultHelp mode.");
        if (Status != ChatStatus.Active)
            throw new DomainException($"Cannot transition to FinalAnswerSent from {Status}.");

        Status = ChatStatus.FinalAnswerSent;
        AllowOneAdditionalQuestion = true;
    }

    /// <summary>
    /// The optional follow-up question was answered; close the chat.
    /// </summary>
    public void Complete()
    {
        if (Status == ChatStatus.Completed)
            throw new DomainException("Chat is already completed.");

        Status = ChatStatus.Completed;
        AllowOneAdditionalQuestion = false;
    }

    /// <summary>Returns true when the chat can still accept user messages.</summary>
    public bool CanReceiveMessage() =>
        Status is ChatStatus.Active or ChatStatus.AwaitingUserAnswers or ChatStatus.FinalAnswerSent
        && !(Status == ChatStatus.FinalAnswerSent && !AllowOneAdditionalQuestion);
}
