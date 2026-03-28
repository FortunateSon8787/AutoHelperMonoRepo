using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Domain.Chats;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.Chat;

namespace AutoHelper.Infrastructure.Ai;

/// <summary>
/// ILlmProvider implementation backed by the OpenAI Chat Completions API.
/// The API key is read from configuration and NEVER exposed to the client.
/// </summary>
public sealed class OpenAiLlmProvider(
    IOptions<LlmSettings> options,
    ILogger<OpenAiLlmProvider> logger) : ILlmProvider
{
    // ChatClient wraps HttpClient and is thread-safe — reuse a single instance to avoid socket exhaustion.
    private readonly ChatClient _chatClient = new(options.Value.Model, options.Value.ApiKey);

    // ─── System prompts ───────────────────────────────────────────────────────

    private static string BuildSystemPrompt(ChatMode mode, string locale, string? vehicleContext)
    {
        var vehicleSection = vehicleContext is not null
            ? $"\nVehicle context: {vehicleContext}"
            : string.Empty;

        var modeInstructions = mode switch
        {
            ChatMode.FaultHelp =>
                "You are an expert automotive diagnostician. " +
                "Help the user diagnose vehicle faults, describe possible causes, and recommend actions. " +
                "Only answer questions related to vehicle faults, symptoms, or diagnostic procedures.",

            ChatMode.WorkClarification =>
                "You are a senior automotive technician. " +
                "Explain service operations, maintenance procedures, and why certain work is necessary. " +
                "Only answer questions related to vehicle maintenance or service record clarifications.",

            ChatMode.PartnerAdvice =>
                "You are an automotive services advisor. " +
                "Help the user find appropriate service providers, explain what type of specialist they need, " +
                "and describe what services different partner types offer. " +
                "Only answer questions related to finding or choosing automotive service partners.",

            _ => "You are an automotive assistant. Only answer questions related to cars and automotive services."
        };

        return $"{modeInstructions}{vehicleSection}\n\nAlways reply in the language: {locale}.";
    }

    private static string BuildTopicGuardPrompt(ChatMode mode) =>
        $"You are a topic guard for an automotive assistant in {mode} mode. " +
        "Respond with exactly 'yes' if the user message is on-topic for automotive questions, " +
        "or exactly 'no' if it is off-topic or unrelated to cars and automotive services.";

    // ─── ILlmProvider ─────────────────────────────────────────────────────────

    public async Task<string> SendAsync(
        ChatMode mode,
        IReadOnlyList<LlmMessage> history,
        string userMessage,
        string locale,
        string? vehicleContext,
        CancellationToken ct)
    {
        var messages = new List<OpenAI.Chat.ChatMessage>
        {
            OpenAI.Chat.ChatMessage.CreateSystemMessage(BuildSystemPrompt(mode, locale, vehicleContext))
        };

        foreach (var msg in history)
        {
            messages.Add(msg.Role == "user"
                ? OpenAI.Chat.ChatMessage.CreateUserMessage(msg.Content)
                : OpenAI.Chat.ChatMessage.CreateAssistantMessage(msg.Content));
        }

        messages.Add(OpenAI.Chat.ChatMessage.CreateUserMessage(userMessage));

        try
        {
            var completion = await _chatClient.CompleteChatAsync(messages, cancellationToken: ct);
            return completion.Value.Content[0].Text;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "LLM provider call failed for mode {Mode}", mode);
            throw;
        }
    }

    public async Task<bool> IsOnTopicAsync(ChatMode mode, string userMessage, CancellationToken ct)
    {

        var messages = new List<OpenAI.Chat.ChatMessage>
        {
            OpenAI.Chat.ChatMessage.CreateSystemMessage(BuildTopicGuardPrompt(mode)),
            OpenAI.Chat.ChatMessage.CreateUserMessage(userMessage)
        };

        try
        {
            var completion = await _chatClient.CompleteChatAsync(messages, cancellationToken: ct);
            var answer = completion.Value.Content[0].Text.Trim().ToLowerInvariant();
            return answer.StartsWith("yes");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Topic guard call failed for mode {Mode}", mode);
            // Fail open — if topic guard is unavailable, allow the message through
            return true;
        }
    }
}
