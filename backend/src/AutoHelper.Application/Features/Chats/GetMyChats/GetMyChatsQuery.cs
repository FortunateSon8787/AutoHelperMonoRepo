using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.Chats.GetMyChats;

public sealed record GetMyChatsQuery : IRequest<Result<IReadOnlyList<ChatSummaryResponse>>>;
