using AutoHelper.Domain.Customers;

namespace AutoHelper.Application.Common.Interfaces;

/// <summary>
/// Provides data access operations for RefreshToken entities.
/// </summary>
public interface IRefreshTokenRepository
{
    /// <summary>Finds an active refresh token by its value.</summary>
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct);

    /// <summary>Returns all refresh tokens belonging to a customer.</summary>
    Task<IList<RefreshToken>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct);

    /// <summary>Adds a new refresh token (tracked, not yet persisted).</summary>
    void Add(RefreshToken refreshToken);
}
