namespace AutoHelper.Domain.Chats;

/// <summary>
/// The initial form submitted by the user when starting a FaultHelp (Mode 1) chat session.
/// Provides the LLM with structured starting context for the diagnostic conversation.
/// </summary>
public sealed record DiagnosticsInput
{
    /// <summary>Free-text description of symptoms the user is experiencing.</summary>
    public string Symptoms { get; init; } = string.Empty;

    /// <summary>Recent suspicious actions or situations the user noticed (optional).</summary>
    public string? RecentEvents { get; init; }

    /// <summary>Previously occurred issues the user recalls (optional).</summary>
    public string? PreviousIssues { get; init; }
}
