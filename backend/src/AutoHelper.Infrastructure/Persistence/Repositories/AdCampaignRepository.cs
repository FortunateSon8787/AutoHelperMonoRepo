using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Domain.AdCampaigns;
using AutoHelper.Domain.Partners;
using Microsoft.EntityFrameworkCore;

namespace AutoHelper.Infrastructure.Persistence.Repositories;

public sealed class AdCampaignRepository(AppDbContext db) : IAdCampaignRepository
{
    public Task<AdCampaign?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.AdCampaigns.FindAsync([id], ct).AsTask();

    public Task<IReadOnlyList<AdCampaign>> GetByPartnerIdAsync(Guid partnerId, CancellationToken ct) =>
        db.AdCampaigns
            .Where(c => c.PartnerId == partnerId)
            .OrderByDescending(c => c.StartsAt)
            .ToListAsync(ct)
            .ContinueWith(t => (IReadOnlyList<AdCampaign>)t.Result, ct);

    public Task<IReadOnlyList<AdCampaign>> GetActiveForDisplayAsync(
        bool isAuthenticated,
        PartnerType? targetCategory,
        CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        var query = db.AdCampaigns
            .Where(c => c.IsActive && c.StartsAt <= now && c.EndsAt >= now);

        if (targetCategory.HasValue)
            query = query.Where(c => c.TargetCategory == targetCategory.Value);

        // Anonymous users: only campaigns with ShowToAnonymous = true
        if (!isAuthenticated)
            query = query.Where(c => c.ShowToAnonymous);

        return query
            .ToListAsync(ct)
            .ContinueWith(t => (IReadOnlyList<AdCampaign>)t.Result, ct);
    }

    public Task<IReadOnlyList<AdCampaign>> GetActiveByPartnerIdAsync(Guid partnerId, CancellationToken ct) =>
        db.AdCampaigns
            .Where(c => c.PartnerId == partnerId && c.IsActive)
            .ToListAsync(ct)
            .ContinueWith(t => (IReadOnlyList<AdCampaign>)t.Result, ct);

    public void Add(AdCampaign campaign) =>
        db.AdCampaigns.Add(campaign);

    public async Task<(IReadOnlyList<AdCampaign> Items, int TotalCount)> GetPagedForAdminAsync(
        int page, int pageSize, Guid? partnerId, CancellationToken ct)
    {
        var query = db.AdCampaigns.Where(c => !c.IsDeleted);

        if (partnerId.HasValue)
            query = query.Where(c => c.PartnerId == partnerId.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(c => c.StartsAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }
}
