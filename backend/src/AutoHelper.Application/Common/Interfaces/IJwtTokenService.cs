using AutoHelper.Domain.Admins;
using AutoHelper.Domain.Customers;

namespace AutoHelper.Application.Common.Interfaces;

/// <summary>
/// Generates JWT access tokens and opaque refresh tokens for authenticated users.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Creates a signed JWT access token containing customer claims.
    /// Token validity is configured via JwtSettings.AccessTokenExpiryMinutes.
    /// </summary>
    string GenerateAccessToken(Customer customer);

    /// <summary>
    /// Creates a signed JWT access token for an admin user with a role claim.
    /// Token validity is configured via JwtSettings.AccessTokenExpiryMinutes.
    /// </summary>
    string GenerateAdminAccessToken(AdminUser adminUser);

    /// <summary>
    /// Creates a cryptographically random opaque refresh token (base64, 64 bytes).
    /// Validity period is configured via JwtSettings.RefreshTokenExpiryDays.
    /// </summary>
    string GenerateRefreshToken();

    /// <summary>Returns the configured refresh token expiry in days.</summary>
    int RefreshTokenExpiryDays { get; }
}
