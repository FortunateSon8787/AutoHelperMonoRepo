using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.Chats.SendMessage;

/// <param name="ChatId">The chat session to send the message to.</param>
/// <param name="Content">User's message text.</param>
/// <param name="Locale">UI locale used to instruct the LLM to reply in the correct language.</param>
public sealed record SendMessageCommand(
    Guid ChatId,
    string Content,
    string Locale = "ru") : IRequest<Result<SendMessageResponse>>;
