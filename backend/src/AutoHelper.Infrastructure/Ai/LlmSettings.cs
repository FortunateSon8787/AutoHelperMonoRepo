namespace AutoHelper.Infrastructure.Ai;

public sealed class LlmSettings
{
    public const string SectionName = "LLM";

    public string ApiKey { get; init; } = string.Empty;

    /// <summary>Fast nano model used for request classification (routing step).</summary>
    public string RouterModel { get; init; } = "gpt-4.1-nano";

    /// <summary>Default model used for response generation when no escalation is needed.</summary>
    public string DefaultModel { get; init; } = "gpt-4.1-mini";

    /// <summary>Escalation model used when the classifier sets should_escalate = true.</summary>
    public string EscalationModel { get; init; } = "gpt-4.1";
}
