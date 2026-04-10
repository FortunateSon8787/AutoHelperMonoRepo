using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using MediatR;

namespace AutoHelper.Application.Features.Chats.GetMyChats;

public sealed class GetMyChatsQueryHandler(
    IChatRepository chats,
    ICurrentUser currentUser) : IRequestHandler<GetMyChatsQuery, Result<PagedResult<ChatSummaryResponse>>>
{
    public async Task<Result<PagedResult<ChatSummaryResponse>>> Handle(
        GetMyChatsQuery request,
        CancellationToken ct)
    {
        if (currentUser.Id is null)
            return AppErrors.Auth.NotAuthenticated;

        var paged = await chats.GetPagedSummariesByCustomerIdAsync(
            currentUser.Id.Value, request.Page, request.PageSize, ct);

        var mappedItems = paged.Items
            .Select(ChatSummaryResponse.FromSummary)
            .ToList()
            .AsReadOnly();

        var response = new PagedResult<ChatSummaryResponse>(
            mappedItems, paged.TotalCount, paged.Page, paged.PageSize);

        return Result<PagedResult<ChatSummaryResponse>>.Success(response);
    }
}
