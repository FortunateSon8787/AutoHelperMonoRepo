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

    /// <summary>Returns all active and verified partners for geographic proximity search.</summary>
    Task<IReadOnlyList<Partner>> SearchByLocationAsync(CancellationToken ct);

    /// <summary>
    /// Returns active, verified partners of the given type within the specified radius (in km).
    /// Includes IsOpenNow logic based on the current UTC time.
    /// </summary>
    Task<IReadOnlyList<Partner>> SearchByTypeAndLocationAsync(
        PartnerType type,
        double lat,
        double lng,
        double radiusKm,
        CancellationToken ct);

    /// <summary>Returns all partners pending administrator verification.</summary>
    Task<IReadOnlyList<Partner>> GetPendingVerificationAsync(CancellationToken ct);

    /// <summary>Returns a paged list of all partners (including unverified/inactive) for admin use, with optional search by name or address.</summary>
    Task<(IReadOnlyList<Partner> Items, int TotalCount)> GetPagedForAdminAsync(int page, int pageSize, string? search, CancellationToken ct);

    /// <summary>Returns all partners flagged as potentially unfit (IsPotentiallyUnfit = true).</summary>
    Task<IReadOnlyList<Partner>> GetPotentiallyUnfitAsync(CancellationToken ct);

    /// <summary>Adds a new partner to the repository (tracked, not yet persisted).</summary>
    void Add(Partner partner);
}
