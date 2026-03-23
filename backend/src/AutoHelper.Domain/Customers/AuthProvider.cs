namespace AutoHelper.Domain.Customers;

/// <summary>
/// Identifies how a customer authenticates with the system.
/// </summary>
public enum AuthProvider
{
    /// <summary>Standard email + password authentication.</summary>
    Local,

    /// <summary>Authentication via Google OAuth 2.0.</summary>
    Google
}
