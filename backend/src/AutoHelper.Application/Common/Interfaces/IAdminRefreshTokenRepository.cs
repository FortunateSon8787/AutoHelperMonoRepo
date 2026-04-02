using AutoHelper.Domain.Admins;

namespace AutoHelper.Application.Common.Interfaces;

/// <summary>
/// Provides data access operations for AdminRefreshToken entities.
/// </summary>
public interface IAdminRefreshTokenRepository
{
    /// <summary>Finds a refresh token by its value.</summary>
    Task<AdminRefreshToken?> GetByTokenAsync(string token, CancellationToken ct);

    /// <summary>Adds a new refresh token (tracked, not yet persisted).</summary>
    void Add(AdminRefreshToken refreshToken);
}
