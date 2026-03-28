using AutoHelper.Domain.Chats;

namespace AutoHelper.Application.Features.Chats.GetChatMessages;

public sealed record MessageResponse(
    Guid Id,
    MessageRole Role,
    string Content,
    bool IsValid,
    DateTime CreatedAt)
{
    public static MessageResponse FromMessage(Message message) =>
        new(message.Id,
            message.Role,
            message.Content,
            message.IsValid,
            message.CreatedAt);
}
