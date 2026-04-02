using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Domain.Admins;
using Microsoft.EntityFrameworkCore;

namespace AutoHelper.Infrastructure.Persistence.Repositories;

public sealed class AdminRefreshTokenRepository(AppDbContext db) : IAdminRefreshTokenRepository
{
    public Task<AdminRefreshToken?> GetByTokenAsync(string token, CancellationToken ct) =>
        db.AdminRefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token, ct);

    public void Add(AdminRefreshToken refreshToken) =>
        db.AdminRefreshTokens.Add(refreshToken);
}
