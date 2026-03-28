using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Application.Features.Chats;
using MediatR;

namespace AutoHelper.Application.Features.Chats.GetMyChats;

public sealed class GetMyChatsQueryHandler(
    IChatRepository chats,
    ICurrentUser currentUser) : IRequestHandler<GetMyChatsQuery, Result<IReadOnlyList<ChatSummaryResponse>>>
{
    public async Task<Result<IReadOnlyList<ChatSummaryResponse>>> Handle(
        GetMyChatsQuery request,
        CancellationToken ct)
    {
        if (currentUser.Id is null)
            return Result<IReadOnlyList<ChatSummaryResponse>>.Failure(ChatErrors.NotAuthenticated);

        var summaries = await chats.GetSummariesByCustomerIdAsync(currentUser.Id.Value, ct);

        var response = summaries
            .Select(ChatSummaryResponse.FromSummary)
            .ToList()
            .AsReadOnly();

        return Result<IReadOnlyList<ChatSummaryResponse>>.Success(response);
    }
}
