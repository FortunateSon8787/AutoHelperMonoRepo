namespace AutoHelper.Application.Features.Chats.SendMessage;

/// <param name="AssistantReply">The text returned by the LLM.</param>
/// <param name="WasValid">True when the message was on-topic and processed normally.</param>
public sealed record SendMessageResponse(string AssistantReply, bool WasValid);
