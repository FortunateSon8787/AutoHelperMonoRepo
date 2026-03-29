namespace AutoHelper.Domain.Chats;

/// <summary>
/// Lifecycle status of a chat session.
/// Only FaultHelp (Mode 1) uses the full state machine; other modes stay Active → Completed.
/// </summary>
public enum ChatStatus
{
    /// <summary>Chat is open and accepts user messages.</summary>
    Active,

    /// <summary>Bot asked follow-up questions; waiting for user to answer them (FaultHelp only).</summary>
    AwaitingUserAnswers,

    /// <summary>Final diagnostic result was sent; one additional user question is still allowed (FaultHelp only).</summary>
    FinalAnswerSent,

    /// <summary>Chat is closed and read-only.</summary>
    Completed
}
