namespace AutoHelper.Domain.Customers;

/// <summary>
/// Paid subscription plan tiers available on AutoHelper.
/// </summary>
public enum SubscriptionPlan
{
    /// <summary>No active paid plan — free tier only.</summary>
    None,

    /// <summary>Normal plan — $4.99/mo, 30 AI requests per month.</summary>
    Normal,

    /// <summary>Pro plan — $7.99/mo, 100 AI requests per month.</summary>
    Pro,

    /// <summary>Max plan — $12.99/mo, 300 AI requests per month.</summary>
    Max
}
