using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Application.Features.Chats.Orchestration;
using AutoHelper.Domain.Chats;
using AutoHelper.Domain.Customers;
using MediatR;

namespace AutoHelper.Application.Features.Chats.SendMessage;

public sealed class SendMessageCommandHandler(
    IChatRepository chats,
    ICustomerRepository customers,
    AutoAssistantOrchestrator orchestrator,
    ICurrentUser currentUser) : IRequestHandler<SendMessageCommand, Result<SendMessageResponse>>
{
    public async Task<Result<SendMessageResponse>> Handle(SendMessageCommand request, CancellationToken ct)
    {
        if (currentUser.Id is null)
            return AppErrors.Auth.NotAuthenticated;

        var customer = await customers.GetByIdAsync(currentUser.Id.Value, ct);
        if (customer is null)
            return AppErrors.Chat.CustomerNotFound;

        if (customer.IsBlocked)
            return AppErrors.Chat.CustomerBlocked;

        var chat = await chats.GetByIdAsync(request.ChatId, includeMessages: true, ct);
        if (chat is null || chat.CustomerId != currentUser.Id.Value)
            return AppErrors.Chat.NotFound;

        if (!chat.CanReceiveMessage())
            return AppErrors.Chat.ChatIsCompleted;

        if (!CanSendMessage(customer, chat.Mode))
            return AppErrors.Chat.SubscriptionRequired;

        if (RequiresQuota(chat.Mode) && customer.AiRequestsRemaining <= 0)
            return AppErrors.Chat.QuotaExceeded;

        var result = await orchestrator.ProcessAsync(chat, customer, request.Content, request.Locale, ct);

        return Result<SendMessageResponse>.Success(
            new SendMessageResponse(
                result.AssistantReply,
                WasValid: result.WasValid,
                ResponseStage: result.ResponseStage,
                ChatStatus: result.ChatStatus,
                DiagnosticResultJson: result.DiagnosticResultJson));
    }

    private static bool CanSendMessage(Customer customer, ChatMode mode) =>
        mode == ChatMode.PartnerAdvice
        || customer.SubscriptionStatus == SubscriptionStatus.Premium;

    private static bool RequiresQuota(ChatMode mode) =>
        mode is ChatMode.FaultHelp or ChatMode.WorkClarification;
}
