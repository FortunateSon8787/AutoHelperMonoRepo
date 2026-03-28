using AutoHelper.Domain.Partners;

namespace AutoHelper.Application.Common.Interfaces;

/// <summary>
/// Provides data access operations for the Partner aggregate.
/// </summary>
public interface IPartnerRepository
{
    /// <summary>Finds a partner by their unique identifier.</summary>
    Task<Partner?> GetByIdAsync(Guid id, CancellationToken ct);

    /// <summary>Finds a partner by the owning user account identifier.</summary>
    Task<Partner?> GetByAccountUserIdAsync(Guid accountUserId, CancellationToken ct);

    /// <summary>Checks whether a user account already has a partner profile.</summary>
    Task<bool> ExistsByAccountUserIdAsync(Guid accountUserId, CancellationToken ct);

    /// <summary>Returns all partners (active and verified) for public listing.</summary>
    Task<IReadOnlyList<Partner>> GetAllVerifiedAsync(CancellationToken ct);

    /// <summary>Returns all partners pending administrator verification.</summary>
    Task<IReadOnlyList<Partner>> GetPendingVerificationAsync(CancellationToken ct);

    /// <summary>Adds a new partner to the repository (tracked, not yet persisted).</summary>
    void Add(Partner partner);
}
