using AutoHelper.Domain.Common;

namespace AutoHelper.Domain.Customers;

/// <summary>
/// Represents a refresh token issued to a customer.
/// Stored in the database for validation and revocation on logout.
/// </summary>
public sealed class RefreshToken : Entity<Guid>
{
    public string Token { get; private set; } = string.Empty;
    public Guid CustomerId { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; }
    public DateTime CreatedAt { get; private set; }

    /// <summary>Token is expired if current time exceeds ExpiresAt.</summary>
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;

    /// <summary>Token is active only when not revoked and not expired.</summary>
    public bool IsActive => !IsRevoked && !IsExpired;

    // ─── EF Core ──────────────────────────────────────────────────────────────

    private RefreshToken() { }

    // ─── Factory ──────────────────────────────────────────────────────────────

    public static RefreshToken Create(Guid customerId, string token, int expiryDays) =>
        new()
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(expiryDays),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };

    // ─── Business operations ──────────────────────────────────────────────────

    /// <summary>Revokes this token (e.g., on logout or token rotation).</summary>
    public void Revoke()
    {
        IsRevoked = true;
    }
}
