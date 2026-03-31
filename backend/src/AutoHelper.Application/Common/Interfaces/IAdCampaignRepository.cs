using AutoHelper.Domain.AdCampaigns;
using AutoHelper.Domain.Partners;

namespace AutoHelper.Application.Common.Interfaces;

/// <summary>
/// Provides data access operations for the AdCampaign aggregate.
/// </summary>
public interface IAdCampaignRepository
{
    /// <summary>Finds a campaign by its unique identifier.</summary>
    Task<AdCampaign?> GetByIdAsync(Guid id, CancellationToken ct);

    /// <summary>Returns all campaigns belonging to a specific partner.</summary>
    Task<IReadOnlyList<AdCampaign>> GetByPartnerIdAsync(Guid partnerId, CancellationToken ct);

    /// <summary>
    /// Returns active campaigns that are currently running and visible to the given visitor.
    /// Optionally filters by service category for targeting.
    /// </summary>
    Task<IReadOnlyList<AdCampaign>> GetActiveForDisplayAsync(
        bool isAuthenticated,
        PartnerType? targetCategory,
        CancellationToken ct);

    /// <summary>Returns all active campaigns belonging to a specific partner.</summary>
    Task<IReadOnlyList<AdCampaign>> GetActiveByPartnerIdAsync(Guid partnerId, CancellationToken ct);

    /// <summary>Adds a new campaign to the repository (tracked, not yet persisted).</summary>
    void Add(AdCampaign campaign);
}
