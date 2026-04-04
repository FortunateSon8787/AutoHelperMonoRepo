using AutoHelper.Domain.Customers;

namespace AutoHelper.Application.Features.Admin.ChatbotConfig;

public sealed record ChatbotConfigResponse(
    bool IsEnabled,
    int MaxCharsPerField,
    Dictionary<string, int> DailyLimitByPlan,
    decimal TopUpPriceUsd,
    int TopUpRequestCount,
    bool DisablePartnerSuggestionsInMode1);
