namespace AutoHelper.Application.Common.Interfaces;

/// <summary>
/// Provides secure password hashing and verification.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>Creates a cryptographic hash of the plain-text password.</summary>
    string Hash(string password);

    /// <summary>
    /// Verifies a plain-text password against a previously hashed value.
    /// Uses constant-time comparison to prevent timing attacks.
    /// </summary>
    bool Verify(string password, string passwordHash);
}
