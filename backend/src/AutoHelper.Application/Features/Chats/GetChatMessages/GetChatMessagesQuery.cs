using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.Chats.GetChatMessages;

public sealed record GetChatMessagesQuery(Guid ChatId) : IRequest<Result<IReadOnlyList<MessageResponse>>>;
