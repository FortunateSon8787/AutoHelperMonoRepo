using AutoHelper.Domain.Common;
using AutoHelper.Domain.Customers;

namespace AutoHelper.Domain.Chatbot;

/// <summary>
/// Singleton configuration record for the AutoHelper AI chatbot.
/// One row exists in the database; use the fixed ID <see cref="SingletonId"/> for seeding.
/// </summary>
public sealed class ChatbotConfig : Entity<Guid>
{
    /// <summary>Fixed ID used for the singleton seed row.</summary>
    public static readonly Guid SingletonId = new("cccccccc-0000-0000-0000-000000000001");

    /// <summary>Whether the chatbot is enabled. When false the chat UI shows a maintenance message.</summary>
    public bool IsEnabled { get; private set; }

    /// <summary>Maximum number of characters allowed in any single user-input field.</summary>
    public int MaxCharsPerField { get; private set; }

    /// <summary>
    /// Daily AI request limit per subscription plan tier.
    /// Stored as JSON in the DB column <c>daily_limit_by_plan</c>.
    /// </summary>
    public Dictionary<SubscriptionPlan, int> DailyLimitByPlan { get; private set; } = [];

    /// <summary>Price in USD for a single top-up purchase.</summary>
    public decimal TopUpPriceUsd { get; private set; }

    /// <summary>Number of AI requests granted per top-up purchase.</summary>
    public int TopUpRequestCount { get; private set; }

    /// <summary>When true, partner suggestion cards are not appended in Mode 1 (diagnostics) responses.</summary>
    public bool DisablePartnerSuggestionsInMode1 { get; private set; }

    // Required by EF Core
    private ChatbotConfig() { }

    private ChatbotConfig(
        Guid id,
        bool isEnabled,
        int maxCharsPerField,
        Dictionary<SubscriptionPlan, int> dailyLimitByPlan,
        decimal topUpPriceUsd,
        int topUpRequestCount,
        bool disablePartnerSuggestionsInMode1) : base(id)
    {
        IsEnabled = isEnabled;
        MaxCharsPerField = maxCharsPerField;
        DailyLimitByPlan = dailyLimitByPlan;
        TopUpPriceUsd = topUpPriceUsd;
        TopUpRequestCount = topUpRequestCount;
        DisablePartnerSuggestionsInMode1 = disablePartnerSuggestionsInMode1;
    }

    public static ChatbotConfig CreateDefault() => new(
        id: SingletonId,
        isEnabled: true,
        maxCharsPerField: 2000,
        dailyLimitByPlan: new Dictionary<SubscriptionPlan, int>
        {
            [SubscriptionPlan.None]   = 0,
            [SubscriptionPlan.Normal] = 10,
            [SubscriptionPlan.Pro]    = 20,
            [SubscriptionPlan.Max]    = 40
        },
        topUpPriceUsd: 3.00m,
        topUpRequestCount: 10,
        disablePartnerSuggestionsInMode1: false);

    public void Update(
        bool isEnabled,
        int maxCharsPerField,
        Dictionary<SubscriptionPlan, int> dailyLimitByPlan,
        decimal topUpPriceUsd,
        int topUpRequestCount,
        bool disablePartnerSuggestionsInMode1)
    {
        IsEnabled = isEnabled;
        MaxCharsPerField = maxCharsPerField;
        DailyLimitByPlan = dailyLimitByPlan;
        TopUpPriceUsd = topUpPriceUsd;
        TopUpRequestCount = topUpRequestCount;
        DisablePartnerSuggestionsInMode1 = disablePartnerSuggestionsInMode1;
    }
}
