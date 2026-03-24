using AutoHelper.Domain.Common;
using AutoHelper.Domain.Customers.Events;

namespace AutoHelper.Domain.Customers;

/// <summary>
/// Aggregate root representing a registered customer.
/// Supports both local (email + password) and Google OAuth authentication.
/// </summary>
public sealed class Customer : AggregateRoot<Guid>
{
    // ─── Core fields ──────────────────────────────────────────────────────────

    public string Name { get; private set; } = string.Empty;

    /// <summary>Primary email address (normalized to lowercase).</summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>PBKDF2 password hash. Null for Google OAuth customers.</summary>
    public string? PasswordHash { get; private set; }

    /// <summary>Optional contact information (phone number, Telegram, etc.).</summary>
    public string? Contacts { get; private set; }

    public SubscriptionStatus SubscriptionStatus { get; private set; }

    public DateTime RegistrationDate { get; private set; }

    // ─── Auth provider ────────────────────────────────────────────────────────

    public AuthProvider AuthProvider { get; private set; }

    // ─── Google OAuth fields ──────────────────────────────────────────────────

    /// <summary>Google's unique user identifier (sub claim).</summary>
    public string? GoogleId { get; private set; }

    /// <summary>Email from Google account (may differ from primary Email).</summary>
    public string? GoogleEmail { get; private set; }

    /// <summary>URL of the Google profile picture.</summary>
    public string? GooglePicture { get; private set; }

    /// <summary>Google OAuth refresh token for offline access.</summary>
    public string? GoogleRefreshToken { get; private set; }

    // ─── EF Core ──────────────────────────────────────────────────────────────

    private Customer() { }

    // ─── Factory methods ──────────────────────────────────────────────────────

    /// <summary>
    /// Creates a customer who authenticates with email and password.
    /// </summary>
    public static Customer CreateWithPassword(
        string name,
        string email,
        string passwordHash,
        string? contacts = null)
    {
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Name = name,
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            Contacts = contacts,
            SubscriptionStatus = SubscriptionStatus.Free,
            RegistrationDate = DateTime.UtcNow,
            AuthProvider = AuthProvider.Local
        };

        customer.AddDomainEvent(new CustomerRegisteredEvent(customer.Id, customer.Email));
        return customer;
    }

    /// <summary>
    /// Creates a customer who authenticates via Google OAuth.
    /// </summary>
    public static Customer CreateWithGoogle(
        string name,
        string email,
        string googleId,
        string? googleEmail,
        string? googlePicture,
        string? googleRefreshToken,
        string? contacts = null)
    {
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Name = name,
            Email = email.Trim().ToLowerInvariant(),
            Contacts = contacts,
            SubscriptionStatus = SubscriptionStatus.Free,
            RegistrationDate = DateTime.UtcNow,
            AuthProvider = AuthProvider.Google,
            GoogleId = googleId,
            GoogleEmail = googleEmail?.Trim().ToLowerInvariant(),
            GooglePicture = googlePicture,
            GoogleRefreshToken = googleRefreshToken
        };

        customer.AddDomainEvent(new CustomerRegisteredEvent(customer.Id, customer.Email));
        return customer;
    }

    // ─── Business operations ──────────────────────────────────────────────────

    /// <summary>
    /// Updates Google-specific profile information received from the OAuth callback.
    /// </summary>
    public void UpdateGoogleInfo(string? googlePicture, string? googleRefreshToken)
    {
        GooglePicture = googlePicture;

        // Only update if a new token was issued (Google may not always return it)
        if (googleRefreshToken is not null)
            GoogleRefreshToken = googleRefreshToken;
    }

    /// <summary>Updates optional contact information.</summary>
    public void UpdateContacts(string? contacts)
    {
        Contacts = contacts;
    }

    /// <summary>Updates the customer's display name and contact information.</summary>
    public void UpdateProfile(string name, string? contacts)
    {
        Name = name;
        Contacts = contacts;
    }

    /// <summary>
    /// Replaces the password hash with a new one.
    /// Only valid for customers who authenticate locally.
    /// </summary>
    public bool ChangePassword(string newPasswordHash)
    {
        if (AuthProvider != AuthProvider.Local)
            return false;

        PasswordHash = newPasswordHash;
        return true;
    }
}
