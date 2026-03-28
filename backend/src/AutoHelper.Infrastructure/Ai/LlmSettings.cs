namespace AutoHelper.Infrastructure.Ai;

public sealed class LlmSettings
{
    public const string SectionName = "LLM";

    public string ApiKey { get; init; } = string.Empty;
    public string Model { get; init; } = "gpt-4.1";
}
