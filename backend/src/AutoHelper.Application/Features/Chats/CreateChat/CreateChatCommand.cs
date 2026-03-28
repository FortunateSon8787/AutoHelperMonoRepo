using AutoHelper.Application.Common;
using AutoHelper.Domain.Chats;
using MediatR;

namespace AutoHelper.Application.Features.Chats.CreateChat;

/// <param name="Mode">Chat mode selecting the AI assistant behaviour.</param>
/// <param name="Title">Short user-provided title for the session.</param>
/// <param name="VehicleId">Optional vehicle to use as context for the conversation.</param>
public sealed record CreateChatCommand(
    ChatMode Mode,
    string Title,
    Guid? VehicleId = null) : IRequest<Result<Guid>>;
