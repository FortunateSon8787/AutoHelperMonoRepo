using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Domain.Chats;
using AutoHelper.Domain.Customers;
using MediatR;

namespace AutoHelper.Application.Features.Chats.CreateChat;

public sealed class CreateChatCommandHandler(
    IChatRepository chats,
    ICustomerRepository customers,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateChatCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateChatCommand request, CancellationToken ct)
    {
        if (currentUser.Id is null)
            return Result<Guid>.Failure(ChatErrors.NotAuthenticated);

        var customer = await customers.GetByIdAsync(currentUser.Id.Value, ct);
        if (customer is null)
            return Result<Guid>.Failure(ChatErrors.CustomerNotFound);

        if (!CanAccessAiChat(customer, request.Mode))
            return Result<Guid>.Failure(ChatErrors.CreateSubscriptionRequired);

        var chat = Chat.Create(
            customerId: currentUser.Id.Value,
            mode: request.Mode,
            title: request.Title,
            vehicleId: request.VehicleId);

        chats.Add(chat);
        await unitOfWork.SaveChangesAsync(ct);

        return Result<Guid>.Success(chat.Id);
    }

    private static bool CanAccessAiChat(Customer customer, ChatMode mode) =>
        // Mode 3 (PartnerAdvice) is free; Modes 1 and 2 require Premium subscription
        mode == ChatMode.PartnerAdvice
        || customer.SubscriptionStatus == SubscriptionStatus.Premium;
}
