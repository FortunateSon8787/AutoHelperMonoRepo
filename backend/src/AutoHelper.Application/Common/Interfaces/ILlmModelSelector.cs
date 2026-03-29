namespace AutoHelper.Application.Common.Interfaces;

/// <summary>
/// Exposes the three model identifiers configured for the LLM pipeline.
/// The concrete implementation reads from LlmSettings and lives in Infrastructure.
/// </summary>
public interface ILlmModelSelector
{
    /// <summary>Fast nano model for RequestClassifier (routing step).</summary>
    string RouterModel { get; }

    /// <summary>Default model for ResponseGenerator when no escalation is needed.</summary>
    string DefaultModel { get; }

    /// <summary>Escalation model used when the classifier sets should_escalate = true.</summary>
    string EscalationModel { get; }
}
