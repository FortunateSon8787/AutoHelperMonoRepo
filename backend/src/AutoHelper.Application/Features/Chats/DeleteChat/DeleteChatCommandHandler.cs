using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using MediatR;

namespace AutoHelper.Application.Features.Chats.DeleteChat;

public sealed class DeleteChatCommandHandler(
    IChatRepository chats,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteChatCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(DeleteChatCommand request, CancellationToken ct)
    {
        if (currentUser.Id is null)
            return AppErrors.Auth.NotAuthenticated;

        var chat = await chats.GetByIdAsync(request.ChatId, includeMessages: false, ct);

        if (chat is null)
            return AppErrors.Chat.NotFound;

        if (chat.CustomerId != currentUser.Id.Value)
            return AppErrors.Chat.NotFound; // intentionally opaque — don't reveal other users' chats

        chat.SoftDelete();
        await unitOfWork.SaveChangesAsync(ct);

        return Result<Unit>.Success(Unit.Value);
    }
}
