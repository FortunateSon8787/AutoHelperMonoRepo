namespace AutoHelper.Application.Common.Interfaces;

/// <summary>
/// Abstraction over a Large Language Model provider (e.g., OpenAI Responses API).
/// Implementations live in Infrastructure and must never expose the API key to the client.
/// </summary>
public interface ILlmProvider
{
    /// <summary>
    /// Generates a structured JSON response validated against a JSON schema.
    /// Uses the provider's Structured Outputs feature to guarantee schema compliance.
    /// The user input is passed as a separate message — never embedded in systemPrompt.
    /// </summary>
    Task<T> GenerateStructuredAsync<T>(
        string model,
        string systemPrompt,
        string userInput,
        CancellationToken ct)
        where T : class;

    /// <summary>
    /// Generates a structured JSON response using a full conversation history.
    /// Used for multi-turn scenarios (e.g. FaultHelp follow-up questions).
    /// </summary>
    Task<T> GenerateStructuredWithHistoryAsync<T>(
        string model,
        string systemPrompt,
        IReadOnlyList<LlmMessage> conversationHistory,
        CancellationToken ct)
        where T : class;

    /// <summary>
    /// Generates a plain text response from the model.
    /// The user input is passed as a separate message — never embedded in systemPrompt.
    /// </summary>
    Task<string> GenerateTextAsync(
        string model,
        string systemPrompt,
        string userInput,
        CancellationToken ct);

    /// <summary>
    /// Compresses a long conversation history into a concise summary.
    /// Called by ContextAssembler when the message history exceeds the token budget.
    /// </summary>
    Task<string> SummarizeConversationAsync(
        string model,
        IReadOnlyList<LlmMessage> messages,
        CancellationToken ct);
}

/// <summary>Lightweight DTO for passing conversation history to the LLM provider.</summary>
public sealed record LlmMessage(string Role, string Content);
