using AutoHelper.Domain.Admins;
using AutoHelper.Domain.Customers;

namespace AutoHelper.Application.Common.Interfaces;

/// <summary>
/// Generates JWT access tokens and opaque refresh tokens for authenticated users.
/// Client tokens and admin tokens are signed with separate secrets.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Creates a signed JWT access token containing customer claims.
    /// Signed with the client secret. Validity: JwtSettings.AccessTokenExpiryMinutes.
    /// </summary>
    string GenerateAccessToken(Customer customer);

    /// <summary>
    /// Creates a signed JWT access token for an admin user with a role claim.
    /// Signed with the admin secret. Validity: JwtSettings.AdminAccessTokenExpiryMinutes.
    /// </summary>
    string GenerateAdminAccessToken(AdminUser adminUser);

    /// <summary>
    /// Creates a cryptographically random opaque refresh token (base64, 64 bytes).
    /// </summary>
    string GenerateRefreshToken();

    /// <summary>Returns the configured client refresh token expiry in days.</summary>
    int RefreshTokenExpiryDays { get; }

    /// <summary>Returns the configured admin refresh token expiry in days.</summary>
    int AdminRefreshTokenExpiryDays { get; }
}
