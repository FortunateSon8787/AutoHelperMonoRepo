using System.Text.Json;
using AutoHelper.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;

namespace AutoHelper.Infrastructure.Ai;

/// <summary>
/// ILlmProvider implementation backed by the OpenAI Chat Completions API
/// with Structured Outputs support (json_schema response format).
/// Uses a per-model ChatClient to support routing across RouterModel, DefaultModel, and EscalationModel.
/// The API key is read from configuration and NEVER exposed to the client.
/// </summary>
public sealed class OpenAiLlmProvider(
    IOptions<LlmSettings> options,
    ILogger<OpenAiLlmProvider> logger) : ILlmProvider
{
    private readonly LlmSettings _settings = options.Value;

    // OpenAIClient is thread-safe and reuses the underlying HttpClient — create once.
    private readonly OpenAIClient _openAiClient = new(options.Value.ApiKey);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    // ─── ILlmProvider ─────────────────────────────────────────────────────────

    public async Task<T> GenerateStructuredAsync<T>(
        string model,
        string systemPrompt,
        string userInput,
        CancellationToken ct)
        where T : class
    {
        var schema = BuildJsonSchema<T>();
        var responseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
            jsonSchemaFormatName: typeof(T).Name,
            jsonSchema: BinaryData.FromString(schema),
            jsonSchemaIsStrict: true);

        var options = new ChatCompletionOptions { ResponseFormat = responseFormat };

        var messages = new List<ChatMessage>
        {
            ChatMessage.CreateSystemMessage(systemPrompt),
            ChatMessage.CreateUserMessage(userInput)
        };

        try
        {
            var client = _openAiClient.GetChatClient(model);
            var completion = await client.CompleteChatAsync(messages, options, ct);
            var json = completion.Value.Content[0].Text;

            return JsonSerializer.Deserialize<T>(json, JsonOptions)
                ?? throw new InvalidOperationException($"LLM returned null when deserializing {typeof(T).Name}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GenerateStructuredAsync failed for model {Model}, type {Type}", model, typeof(T).Name);
            throw;
        }
    }

    public async Task<string> GenerateTextAsync(
        string model,
        string systemPrompt,
        string userInput,
        CancellationToken ct)
    {
        var messages = new List<ChatMessage>
        {
            ChatMessage.CreateSystemMessage(systemPrompt),
            ChatMessage.CreateUserMessage(userInput)
        };

        try
        {
            var client = _openAiClient.GetChatClient(model);
            var completion = await client.CompleteChatAsync(messages, cancellationToken: ct);
            return completion.Value.Content[0].Text;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GenerateTextAsync failed for model {Model}", model);
            throw;
        }
    }

    public async Task<string> SummarizeConversationAsync(
        string model,
        IReadOnlyList<LlmMessage> messages,
        CancellationToken ct)
    {
        const string systemPrompt =
            "Summarise the following conversation concisely. " +
            "Preserve all vehicle data, fault codes, service operations, and recommendations.";

        var conversationText = string.Join("\n", messages.Select(m => $"{m.Role}: {m.Content}"));

        try
        {
            return await GenerateTextAsync(model, systemPrompt, conversationText, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "SummarizeConversationAsync failed for model {Model}", model);
            throw;
        }
    }

    // ─── JSON Schema builder ──────────────────────────────────────────────────

    /// <summary>
    /// Builds a minimal JSON schema for the given type using its public properties
    /// and their JsonPropertyName attributes. Used by Structured Outputs.
    /// </summary>
    private static string BuildJsonSchema<T>()
    {
        var type = typeof(T);
        var properties = new Dictionary<string, object>();
        var required = new List<string>();

        foreach (var prop in type.GetProperties())
        {
            var jsonAttr = prop.GetCustomAttributes(typeof(System.Text.Json.Serialization.JsonPropertyNameAttribute), false)
                .OfType<System.Text.Json.Serialization.JsonPropertyNameAttribute>()
                .FirstOrDefault();

            var jsonName = jsonAttr?.Name ?? prop.Name;
            var propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

            var jsonType = propType switch
            {
                var t when t == typeof(bool) => "boolean",
                var t when t == typeof(int) || t == typeof(long) => "integer",
                var t when t == typeof(decimal) || t == typeof(double) || t == typeof(float) => "number",
                _ => "string"
            };

            if (Nullable.GetUnderlyingType(prop.PropertyType) is not null || propType == typeof(string))
            {
                properties[jsonName] = new Dictionary<string, object> { ["type"] = new[] { jsonType, "null" } };
            }
            else
            {
                properties[jsonName] = new Dictionary<string, object> { ["type"] = jsonType };
                required.Add(jsonName);
            }
        }

        var schema = new Dictionary<string, object>
        {
            ["type"] = "object",
            ["properties"] = properties,
            ["required"] = required,
            ["additionalProperties"] = false
        };

        return JsonSerializer.Serialize(schema);
    }
}
