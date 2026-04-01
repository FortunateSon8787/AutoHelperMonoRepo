using AutoHelper.Domain.Common;

namespace AutoHelper.Domain.Admins;

/// <summary>
/// Administrator account with a role (Admin or SuperAdmin).
/// Authenticates with email and password.
/// </summary>
public sealed class AdminUser : AggregateRoot<Guid>
{
    public string Email { get; private set; } = string.Empty;

    /// <summary>PBKDF2 password hash.</summary>
    public string PasswordHash { get; private set; } = string.Empty;

    public AdminRole Role { get; private set; }

    // ─── EF Core ──────────────────────────────────────────────────────────────

    private AdminUser() { }

    // ─── Factory ──────────────────────────────────────────────────────────────

    public static AdminUser Create(string email, string passwordHash, AdminRole role)
    {
        return new AdminUser
        {
            Id = Guid.NewGuid(),
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            Role = role
        };
    }

    // ─── Business operations ──────────────────────────────────────────────────

    /// <summary>Replaces the password hash with a new one.</summary>
    public void ChangePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
    }
}
