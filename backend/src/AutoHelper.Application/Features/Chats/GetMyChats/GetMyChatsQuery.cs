using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.Chats.GetMyChats;

public sealed record GetMyChatsQuery(int Page = 1, int PageSize = 20)
    : IRequest<Result<PagedResult<ChatSummaryResponse>>>;
