namespace AutoHelper.Domain.Customers;

/// <summary>
/// Represents the current subscription status of a customer.
/// </summary>
public enum SubscriptionStatus
{
    /// <summary>Free tier — limited access to features.</summary>
    Free,

    /// <summary>Premium tier — full access to all features.</summary>
    Premium,

    /// <summary>Subscription is suspended (e.g., payment failure).</summary>
    Suspended
}
