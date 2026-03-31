using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Domain.Partners;
using AutoHelper.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace AutoHelper.Infrastructure.Persistence.Repositories;

public sealed class PartnerRepository(AppDbContext db) : IPartnerRepository
{
    public Task<Partner?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Partners.FindAsync([id], ct).AsTask();

    public Task<Partner?> GetByAccountUserIdAsync(Guid accountUserId, CancellationToken ct) =>
        db.Partners.FirstOrDefaultAsync(p => p.AccountUserId == accountUserId, ct);

    public Task<bool> ExistsByAccountUserIdAsync(Guid accountUserId, CancellationToken ct) =>
        db.Partners.AnyAsync(p => p.AccountUserId == accountUserId, ct);

    public Task<IReadOnlyList<Partner>> GetAllVerifiedAsync(CancellationToken ct) =>
        db.Partners
            .Where(p => p.IsVerified && p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync(ct)
            .ContinueWith(t => (IReadOnlyList<Partner>)t.Result, ct);

    public Task<IReadOnlyList<Partner>> SearchByLocationAsync(CancellationToken ct) =>
        db.Partners
            .Where(p => p.IsVerified && p.IsActive)
            .ToListAsync(ct)
            .ContinueWith(t => (IReadOnlyList<Partner>)t.Result, ct);

    public Task<IReadOnlyList<Partner>> GetPendingVerificationAsync(CancellationToken ct) =>
        db.Partners
            .Where(p => !p.IsVerified)
            .OrderBy(p => p.Name)
            .ToListAsync(ct)
            .ContinueWith(t => (IReadOnlyList<Partner>)t.Result, ct);

    public async Task<(IReadOnlyList<Partner> Items, int TotalCount)> GetPagedForAdminAsync(
        int page, int pageSize, string? search, CancellationToken ct)
    {
        var query = db.Partners.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(term) ||
                p.Address.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public Task<IReadOnlyList<Partner>> GetPotentiallyUnfitAsync(CancellationToken ct) =>
        db.Partners
            .Where(p => p.IsPotentiallyUnfit)
            .OrderBy(p => p.Name)
            .ToListAsync(ct)
            .ContinueWith(t => (IReadOnlyList<Partner>)t.Result, ct);

    public async Task<IReadOnlyList<Partner>> SearchByTypeAndLocationAsync(
        PartnerType type,
        double lat,
        double lng,
        double radiusKm,
        CancellationToken ct)
    {
        // Load active+verified partners of the requested type.
        // Geographic filtering is done in-memory (same as SearchByLocationAsync)
        // because PostgreSQL doesn't have built-in Haversine; PostGIS is not yet set up.
        var partners = await db.Partners
            .Where(p => p.IsVerified && p.IsActive && p.Type == type)
            .ToListAsync(ct);

        if (radiusKm >= double.MaxValue / 2)
            return partners;

        const double earthRadiusKm = 6371.0;
        return partners.Where(p =>
        {
            var dLat = (p.Location.Lat - lat) * Math.PI / 180.0;
            var dLng = (p.Location.Lng - lng) * Math.PI / 180.0;
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                  + Math.Cos(lat * Math.PI / 180.0) * Math.Cos(p.Location.Lat * Math.PI / 180.0)
                  * Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
            var distanceKm = earthRadiusKm * 2 * Math.Asin(Math.Sqrt(a));
            return distanceKm <= radiusKm;
        }).ToList();
    }

    public void Add(Partner partner) =>
        db.Partners.Add(partner);
}
