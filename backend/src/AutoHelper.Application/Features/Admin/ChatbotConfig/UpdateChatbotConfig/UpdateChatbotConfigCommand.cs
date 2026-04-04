using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.Admin.ChatbotConfig.UpdateChatbotConfig;

public sealed record UpdateChatbotConfigCommand(
    bool IsEnabled,
    int MaxCharsPerField,
    Dictionary<string, int> DailyLimitByPlan,
    decimal TopUpPriceUsd,
    int TopUpRequestCount,
    bool DisablePartnerSuggestionsInMode1) : IRequest<Result>;
