using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using MediatR;
using DomainChatbotConfig = AutoHelper.Domain.Chatbot.ChatbotConfig;

namespace AutoHelper.Application.Features.Admin.ChatbotConfig.GetChatbotConfig;

public sealed class GetChatbotConfigQueryHandler(
    IChatbotConfigRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<GetChatbotConfigQuery, Result<ChatbotConfigResponse>>
{
    public async Task<Result<ChatbotConfigResponse>> Handle(
        GetChatbotConfigQuery request, CancellationToken ct)
    {
        var config = await repository.GetAsync(ct);

        if (config is null)
        {
            // Auto-create default if missing (should not happen after seeding, but defensive)
            config = DomainChatbotConfig.CreateDefault();
            repository.Add(config);
            await unitOfWork.SaveChangesAsync(ct);
        }

        return ToResponse(config);
    }

    private static ChatbotConfigResponse ToResponse(DomainChatbotConfig config) =>
        new(
            config.IsEnabled,
            config.MaxCharsPerField,
            config.DailyLimitByPlan.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value),
            config.TopUpPriceUsd,
            config.TopUpRequestCount,
            config.DisablePartnerSuggestionsInMode1);
}
