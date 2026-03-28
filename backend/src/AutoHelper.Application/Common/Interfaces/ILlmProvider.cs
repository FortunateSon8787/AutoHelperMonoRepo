using AutoHelper.Domain.Chats;

namespace AutoHelper.Application.Common.Interfaces;

/// <summary>
/// Abstraction over a Large Language Model provider (e.g., OpenAI).
/// Implementations live in Infrastructure and must never expose the API key to the client.
/// </summary>
public interface ILlmProvider
{
    /// <summary>
    /// Sends the conversation history to the LLM and returns the assistant's reply.
    /// </summary>
    /// <param name="mode">Chat mode — shapes the system prompt sent to the model.</param>
    /// <param name="history">Ordered list of previous messages in the session (User/Assistant alternating).</param>
    /// <param name="userMessage">The latest user message to respond to.</param>
    /// <param name="locale">UI locale (e.g. "ru", "en") — the model must reply in this language.</param>
    /// <param name="vehicleContext">Optional vehicle details injected into the system prompt.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The assistant's reply text.</returns>
    Task<string> SendAsync(
        ChatMode mode,
        IReadOnlyList<LlmMessage> history,
        string userMessage,
        string locale,
        string? vehicleContext,
        CancellationToken ct);

    /// <summary>
    /// Returns true if the user message is on-topic for the given chat mode.
    /// Used by the topic guard before consuming quota.
    /// </summary>
    Task<bool> IsOnTopicAsync(
        ChatMode mode,
        string userMessage,
        CancellationToken ct);
}

/// <summary>Lightweight DTO for passing conversation history to the LLM provider.</summary>
public sealed record LlmMessage(string Role, string Content);
