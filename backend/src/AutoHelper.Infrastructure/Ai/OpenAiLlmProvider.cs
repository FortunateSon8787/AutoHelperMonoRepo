using System.Text.Json;
using AutoHelper.Application.Common;
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
        var schema = BuildObjectSchema(typeof(T), []);
        return JsonSerializer.Serialize(schema);
    }

    /// <summary>
    /// Recursively builds a JSON Schema object for <paramref name="type"/>.
    /// Handles: primitives, nullable value types, strings, T[] arrays, List&lt;T&gt; / IReadOnlyList&lt;T&gt;
    /// generic collections, and nested object types.
    /// OpenAI Structured Outputs requires ALL properties listed in "required";
    /// optionality is expressed via anyOf with null rather than omitting from required.
    /// <paramref name="visited"/> guards against infinite recursion from self-referential types.
    /// </summary>
    private static Dictionary<string, object> BuildObjectSchema(Type type, HashSet<Type> visited)
    {
        if (!visited.Add(type))
            return new Dictionary<string, object> { ["type"] = "object", ["additionalProperties"] = false };

        var properties = new Dictionary<string, object>();
        var required = new List<string>();

        foreach (var prop in type.GetProperties())
        {
            var jsonAttr = prop.GetCustomAttributes(
                    typeof(System.Text.Json.Serialization.JsonPropertyNameAttribute), false)
                .OfType<System.Text.Json.Serialization.JsonPropertyNameAttribute>()
                .FirstOrDefault();

            var jsonName = jsonAttr?.Name ?? prop.Name;

            // Unwrap Nullable<T> (e.g. int? → int)
            var underlyingType = Nullable.GetUnderlyingType(prop.PropertyType);
            var isNullable = underlyingType is not null || !prop.PropertyType.IsValueType;
            var propType = underlyingType ?? prop.PropertyType;

            // OpenAI Structured Outputs: ALL properties must appear in "required".
            required.Add(jsonName);

            var maxLengthAttr = prop.GetCustomAttributes(typeof(JsonSchemaMaxLengthAttribute), false)
                .OfType<JsonSchemaMaxLengthAttribute>()
                .FirstOrDefault();

            var propSchema = BuildPropertySchema(propType, visited, maxLengthAttr?.MaxLength);

            properties[jsonName] = isNullable
                ? WrapNullable(propSchema)
                : propSchema;
        }

        visited.Remove(type);

        return new Dictionary<string, object>
        {
            ["type"] = "object",
            ["properties"] = properties,
            ["required"] = required,
            ["additionalProperties"] = false
        };
    }

    /// <summary>Builds the schema node for a single property type (non-nullable).</summary>
    private static object BuildPropertySchema(Type propType, HashSet<Type> visited, int? maxLength = null)
    {
        // T[] arrays
        if (propType.IsArray)
        {
            var elementType = propType.GetElementType()!;
            return BuildArraySchema(elementType, visited);
        }

        // Generic collections: List<T>, IReadOnlyList<T>, IEnumerable<T>, IList<T>, etc.
        if (propType.IsGenericType)
        {
            var genericDef = propType.GetGenericTypeDefinition();
            var isCollection =
                genericDef == typeof(List<>) ||
                genericDef == typeof(IList<>) ||
                genericDef == typeof(IReadOnlyList<>) ||
                genericDef == typeof(IEnumerable<>) ||
                genericDef == typeof(ICollection<>) ||
                genericDef == typeof(IReadOnlyCollection<>);

            if (isCollection)
            {
                var elementType = propType.GetGenericArguments()[0];
                return BuildArraySchema(elementType, visited);
            }
        }

        // Primitive / string
        var jsonType = propType switch
        {
            var t when t == typeof(bool) => "boolean",
            var t when t == typeof(int) || t == typeof(long) => "integer",
            var t when t == typeof(decimal) || t == typeof(double) || t == typeof(float) => "number",
            var t when t == typeof(string) => "string",
            _ => null
        };

        if (jsonType is not null)
        {
            var schema = new Dictionary<string, object> { ["type"] = jsonType };
            if (jsonType == "string" && maxLength.HasValue)
                schema["maxLength"] = maxLength.Value;
            return schema;
        }

        // Nested object type — recurse
        return BuildObjectSchema(propType, visited);
    }

    private static Dictionary<string, object> BuildArraySchema(Type elementType, HashSet<Type> visited)
    {
        var itemSchema = IsPrimitive(elementType)
            ? (object)new Dictionary<string, object> { ["type"] = MapPrimitiveType(elementType) }
            : BuildObjectSchema(elementType, visited);

        return new Dictionary<string, object>
        {
            ["type"] = "array",
            ["items"] = itemSchema
        };
    }

    /// <summary>Wraps a schema node in an anyOf with null to express optionality.</summary>
    private static Dictionary<string, object> WrapNullable(object innerSchema) =>
        new()
        {
            ["anyOf"] = new object[]
            {
                innerSchema,
                new Dictionary<string, object> { ["type"] = "null" }
            }
        };

    private static bool IsPrimitive(Type t) =>
        t == typeof(bool) || t == typeof(int) || t == typeof(long) ||
        t == typeof(decimal) || t == typeof(double) || t == typeof(float) ||
        t == typeof(string);

    private static string MapPrimitiveType(Type t) => t switch
    {
        var x when x == typeof(bool) => "boolean",
        var x when x == typeof(int) || x == typeof(long) => "integer",
        var x when x == typeof(decimal) || x == typeof(double) || x == typeof(float) => "number",
        _ => "string"
    };
}
