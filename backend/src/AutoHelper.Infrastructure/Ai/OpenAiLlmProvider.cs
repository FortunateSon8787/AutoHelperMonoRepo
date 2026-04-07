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
            logger.LogInformation("LLM {Model} Request: {Request}", model, JsonSerializer.Serialize(messages));
            
            var client = _openAiClient.GetChatClient(model);
            var completion = await client.CompleteChatAsync(messages, options, ct);
            var json = completion.Value.Content[0].Text;

            logger.LogInformation("LLM {Model} Response: {Response}",model, json);

            return JsonSerializer.Deserialize<T>(json, JsonOptions)
                ?? throw new InvalidOperationException($"LLM returned null when deserializing {typeof(T).Name}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GenerateStructuredAsync failed for model {Model}, type {Type}", model, typeof(T).Name);
            throw;
        }
    }

    public async Task<T> GenerateStructuredWithHistoryAsync<T>(
        string model,
        string systemPrompt,
        IReadOnlyList<LlmMessage> conversationHistory,
        CancellationToken ct)
        where T : class
    {
        var schema = BuildJsonSchema<T>();
        var responseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
            jsonSchemaFormatName: typeof(T).Name,
            jsonSchema: BinaryData.FromString(schema),
            jsonSchemaIsStrict: true);

        var chatOptions = new ChatCompletionOptions { ResponseFormat = responseFormat };

        var messages = new List<ChatMessage>
        {
            ChatMessage.CreateSystemMessage(systemPrompt)
        };

        foreach (var msg in conversationHistory)
        {
            messages.Add(msg.Role == "user"
                ? ChatMessage.CreateUserMessage(msg.Content)
                : ChatMessage.CreateAssistantMessage(msg.Content));
        }

        try
        {
            logger.LogInformation("LLM {Model} Request: {Request}", model, JsonSerializer.Serialize(messages));
            
            var client = _openAiClient.GetChatClient(model);
            var completion = await client.CompleteChatAsync(messages, chatOptions, ct);
            var json = completion.Value.Content[0].Text;

            
            logger.LogInformation("LLM {Model} Response: {Response}",model, json);

            return JsonSerializer.Deserialize<T>(json, JsonOptions)
                ?? throw new InvalidOperationException($"LLM returned null when deserializing {typeof(T).Name}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GenerateStructuredWithHistoryAsync failed for model {Model}, type {Type}", model, typeof(T).Name);
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
            var underlyingType = Nullable.GetUnderlyingType(prop.PropertyType);
            var isNullable = underlyingType is not null
                || !prop.PropertyType.IsValueType; // reference types (string, arrays) are nullable

            var propType = underlyingType ?? prop.PropertyType;

            // OpenAI Structured Outputs requires ALL properties in required.
            // Optionality is expressed via ["type", "null"] union, not by omitting from required.
            required.Add(jsonName);

            if (propType.IsArray)
            {
                var elementType = propType.GetElementType()!;
                var itemSchema = BuildObjectSchema(elementType);
                var arraySchema = new Dictionary<string, object>
                {
                    ["type"] = "array",
                    ["items"] = itemSchema
                };
                properties[jsonName] = isNullable
                    ? (object)new Dictionary<string, object> { ["anyOf"] = new object[] { arraySchema, new Dictionary<string, object> { ["type"] = "null" } } }
                    : arraySchema;
            }
            else
            {
                var jsonType = propType switch
                {
                    var t when t == typeof(bool) => "boolean",
                    var t when t == typeof(int) || t == typeof(long) => "integer",
                    var t when t == typeof(decimal) || t == typeof(double) || t == typeof(float) => "number",
                    _ => "string"
                };

                properties[jsonName] = isNullable
                    ? (object)new Dictionary<string, object> { ["type"] = new[] { jsonType, "null" } }
                    : new Dictionary<string, object> { ["type"] = jsonType };
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

    private static Dictionary<string, object> BuildObjectSchema(Type type)
    {
        var properties = new Dictionary<string, object>();
        var required = new List<string>();

        foreach (var prop in type.GetProperties())
        {
            var jsonAttr = prop.GetCustomAttributes(typeof(System.Text.Json.Serialization.JsonPropertyNameAttribute), false)
                .OfType<System.Text.Json.Serialization.JsonPropertyNameAttribute>()
                .FirstOrDefault();

            var jsonName = jsonAttr?.Name ?? prop.Name;
            var underlyingType = Nullable.GetUnderlyingType(prop.PropertyType);
            var isNullable = underlyingType is not null || !prop.PropertyType.IsValueType;
            var propType = underlyingType ?? prop.PropertyType;

            required.Add(jsonName);

            var jsonType = propType switch
            {
                var t when t == typeof(bool) => "boolean",
                var t when t == typeof(int) || t == typeof(long) => "integer",
                var t when t == typeof(decimal) || t == typeof(double) || t == typeof(float) => "number",
                _ => "string"
            };

            properties[jsonName] = isNullable
                ? (object)new Dictionary<string, object> { ["type"] = new[] { jsonType, "null" } }
                : new Dictionary<string, object> { ["type"] = jsonType };
        }

        return new Dictionary<string, object>
        {
            ["type"] = "object",
            ["properties"] = properties,
            ["required"] = required,
            ["additionalProperties"] = false
        };
    }
}
