using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Domain.Customers;
using MediatR;
using DomainChatbotConfig = AutoHelper.Domain.Chatbot.ChatbotConfig;

namespace AutoHelper.Application.Features.Admin.ChatbotConfig.UpdateChatbotConfig;

public sealed class UpdateChatbotConfigCommandHandler(
    IChatbotConfigRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateChatbotConfigCommand, Result>
{
    public async Task<Result> Handle(UpdateChatbotConfigCommand request, CancellationToken ct)
    {
        var config = await repository.GetAsync(ct);

        if (config is null)
        {
            config = DomainChatbotConfig.CreateDefault();
            repository.Add(config);
        }

        var dailyLimits = ParseDailyLimits(request.DailyLimitByPlan);
        if (dailyLimits is null)
            return AppErrors.ChatbotConfig.InvalidDailyLimitKey;

        config.Update(
            request.IsEnabled,
            request.MaxCharsPerField,
            dailyLimits,
            request.TopUpPriceUsd,
            request.TopUpRequestCount,
            request.DisablePartnerSuggestionsInMode1);

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }

    private static Dictionary<SubscriptionPlan, int>? ParseDailyLimits(Dictionary<string, int> raw)
    {
        var result = new Dictionary<SubscriptionPlan, int>();
        foreach (var (key, value) in raw)
        {
            if (!Enum.TryParse<SubscriptionPlan>(key, ignoreCase: true, out var plan))
                return null;
            result[plan] = value;
        }
        return result;
    }
}
