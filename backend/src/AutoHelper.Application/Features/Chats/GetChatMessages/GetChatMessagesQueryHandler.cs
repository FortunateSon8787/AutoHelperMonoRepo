using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Application.Features.Chats;
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
            return Result<IReadOnlyList<MessageResponse>>.Failure(ChatErrors.NotAuthenticated);

        var chat = await chats.GetByIdAsync(request.ChatId, includeMessages: true, ct);
        if (chat is null || chat.CustomerId != currentUser.Id.Value)
            return Result<IReadOnlyList<MessageResponse>>.Failure(ChatErrors.ChatNotFound);

        var response = chat.Messages
            .OrderBy(m => m.CreatedAt)
            .Select(MessageResponse.FromMessage)
            .ToList()
            .AsReadOnly();

        return Result<IReadOnlyList<MessageResponse>>.Success(response);
    }
}
