using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.Chats.DeleteChat;

public sealed record DeleteChatCommand(Guid ChatId) : IRequest<Result<Unit>>;
