using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Domain.Customers;
using Microsoft.EntityFrameworkCore;

namespace AutoHelper.Infrastructure.Persistence.Repositories;

public sealed class RefreshTokenRepository(AppDbContext db) : IRefreshTokenRepository
{
    public Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct) =>
        db.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token, ct);

    public Task<IList<RefreshToken>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct) =>
        db.RefreshTokens
            .Where(rt => rt.CustomerId == customerId)
            .ToListAsync(ct)
            .ContinueWith<IList<RefreshToken>>(t => t.Result, ct);

    public void Add(RefreshToken refreshToken) =>
        db.RefreshTokens.Add(refreshToken);
}
