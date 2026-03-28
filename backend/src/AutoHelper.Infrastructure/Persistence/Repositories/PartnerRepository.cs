using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Domain.Partners;
using AutoHelper.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

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

    public void Add(Partner partner) =>
        db.Partners.Add(partner);
}
