using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Domain.Chats;

namespace AutoHelper.Application.Features.Chats.GetMyChats;

public sealed record ChatSummaryResponse(
    Guid Id,
    ChatMode Mode,
    ChatStatus Status,
    string Title,
    Guid? VehicleId,
    int MessageCount,
    DateTime CreatedAt)
{
    public static ChatSummaryResponse FromSummary(ChatSummary summary) =>
        new(summary.Id,
            summary.Mode,
            summary.Status,
            summary.Title,
            summary.VehicleId,
            summary.MessageCount,
            summary.CreatedAt);
}
