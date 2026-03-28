using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Domain.Chats;
using AutoHelper.Domain.Customers;
using AutoHelper.Domain.Vehicles;
using MediatR;

namespace AutoHelper.Application.Features.Chats.SendMessage;

public sealed class SendMessageCommandHandler(
    IChatRepository chats,
    ICustomerRepository customers,
    IVehicleRepository vehicles,
    ILlmProvider llm,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork) : IRequestHandler<SendMessageCommand, Result<SendMessageResponse>>
{
    private const string OffTopicReply =
        "Извините, я могу отвечать только на вопросы об автомобилях и автосервисах. " +
        "Пожалуйста, задайте вопрос по теме.";

    public async Task<Result<SendMessageResponse>> Handle(SendMessageCommand request, CancellationToken ct)
    {
        if (currentUser.Id is null)
            return Result<SendMessageResponse>.Failure(ChatErrors.NotAuthenticated);

        var customer = await customers.GetByIdAsync(currentUser.Id.Value, ct);
        if (customer is null)
            return Result<SendMessageResponse>.Failure(ChatErrors.CustomerNotFound);

        var chat = await chats.GetByIdAsync(request.ChatId, includeMessages: true, ct);
        if (chat is null || chat.CustomerId != currentUser.Id.Value)
            return Result<SendMessageResponse>.Failure(ChatErrors.ChatNotFound);

        if (!CanSendMessage(customer, chat.Mode))
            return Result<SendMessageResponse>.Failure(ChatErrors.SubscriptionRequired);

        // Topic guard — off-topic messages are stored but don't consume quota
        var isOnTopic = await llm.IsOnTopicAsync(chat.Mode, request.Content, ct);
        if (!isOnTopic)
        {
            chat.AddInvalidUserMessage(request.Content);
            await unitOfWork.SaveChangesAsync(ct);
            return Result<SendMessageResponse>.Success(new SendMessageResponse(OffTopicReply, WasValid: false));
        }

        var vehicleContext = await BuildVehicleContextAsync(chat.VehicleId, ct);
        var history = BuildLlmHistory(chat.Messages);

        var assistantReply = await llm.SendAsync(
            mode: chat.Mode,
            history: history,
            userMessage: request.Content,
            locale: request.Locale,
            vehicleContext: vehicleContext,
            ct: ct);

        chat.AddExchange(request.Content, assistantReply);
        await unitOfWork.SaveChangesAsync(ct);

        return Result<SendMessageResponse>.Success(new SendMessageResponse(assistantReply, WasValid: true));
    }

    private static bool CanSendMessage(Customer customer, ChatMode mode) =>
        mode == ChatMode.PartnerAdvice
        || customer.SubscriptionStatus == SubscriptionStatus.Premium;

    private async Task<string?> BuildVehicleContextAsync(Guid? vehicleId, CancellationToken ct)
    {
        if (vehicleId is null)
            return null;

        var vehicle = await vehicles.GetByIdAsync(vehicleId.Value, ct);
        if (vehicle is null)
            return null;

        return $"{vehicle.Brand} {vehicle.Model} {vehicle.Year}, {vehicle.Mileage} km, VIN: {vehicle.Vin}";
    }

    private static List<LlmMessage> BuildLlmHistory(IReadOnlyCollection<Message> messages) =>
        messages
            .Where(m => m.IsValid)
            .OrderBy(m => m.CreatedAt)
            .Select(m => new LlmMessage(
                Role: m.Role == MessageRole.User ? "user" : "assistant",
                Content: m.Content))
            .ToList();
}
