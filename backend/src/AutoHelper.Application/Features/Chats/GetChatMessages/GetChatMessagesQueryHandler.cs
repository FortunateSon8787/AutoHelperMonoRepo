using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using MediatR;

namespace AutoHelper.Application.Features.Chats.GetChatMessages;

public sealed class GetChatMessagesQueryHandler(
    IChatRepository chats,
    ICurrentUser currentUser) : IRequestHandler<GetChatMessagesQuery, Result<IReadOnlyList<MessageResponse>>>
{
    public async Task<Result<IReadOnlyList<MessageResponse>>> Handle(
        GetChatMessagesQuery request,
        CancellationToken ct)
    {
        if (currentUser.Id is null)
            return AppErrors.Auth.NotAuthenticated;

        var chat = await chats.GetByIdAsync(request.ChatId, includeMessages: true, ct);
        if (chat is null || chat.CustomerId != currentUser.Id.Value)
            return AppErrors.Chat.NotFound;

        var response = chat.Messages
            .OrderBy(m => m.CreatedAt)
            .Select(MessageResponse.FromMessage)
            .ToList()
            .AsReadOnly();

        return Result<IReadOnlyList<MessageResponse>>.Success(response);
    }
}
