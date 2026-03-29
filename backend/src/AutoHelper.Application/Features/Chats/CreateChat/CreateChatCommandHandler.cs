using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Application.Features.Chats.Orchestration;
using AutoHelper.Domain.Chats;
using AutoHelper.Domain.Customers;
using MediatR;

namespace AutoHelper.Application.Features.Chats.CreateChat;

public sealed class CreateChatCommandHandler(
    IChatRepository chats,
    ICustomerRepository customers,
    AutoAssistantOrchestrator orchestrator,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateChatCommand, Result<CreateChatResponse>>
{
    public async Task<Result<CreateChatResponse>> Handle(CreateChatCommand request, CancellationToken ct)
    {
        if (currentUser.Id is null)
            return Result<CreateChatResponse>.Failure(ChatErrors.NotAuthenticated);

        var customer = await customers.GetByIdAsync(currentUser.Id.Value, ct);
        if (customer is null)
            return Result<CreateChatResponse>.Failure(ChatErrors.CustomerNotFound);

        if (!CanAccessAiChat(customer, request.Mode))
            return Result<CreateChatResponse>.Failure(ChatErrors.CreateSubscriptionRequired);

        if (request.Mode == ChatMode.FaultHelp && request.DiagnosticsInput is null)
            return Result<CreateChatResponse>.Failure(ChatErrors.DiagnosticsInputRequired);

        var chat = Chat.Create(
            customerId: currentUser.Id.Value,
            mode: request.Mode,
            title: request.Title,
            vehicleId: request.VehicleId);

        chats.Add(chat);
        await unitOfWork.SaveChangesAsync(ct);

        // For FaultHelp, immediately process the initial diagnostics form
        if (request.Mode == ChatMode.FaultHelp)
        {
            var locale = "ru"; // Default; will be part of request in future
            var orchResult = await orchestrator.ProcessDiagnosticsInitialAsync(
                chat, customer, request.DiagnosticsInput!, locale, ct);

            return Result<CreateChatResponse>.Success(
                new CreateChatResponse(chat.Id, orchResult.AssistantReply));
        }

        return Result<CreateChatResponse>.Success(new CreateChatResponse(chat.Id, null));
    }

    private static bool CanAccessAiChat(Customer customer, ChatMode mode) =>
        mode == ChatMode.PartnerAdvice
        || customer.SubscriptionStatus == SubscriptionStatus.Premium;
}
