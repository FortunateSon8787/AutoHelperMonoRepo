using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Domain.Admins;
using Microsoft.EntityFrameworkCore;

namespace AutoHelper.Infrastructure.Persistence.Repositories;

public sealed class AdminUserRepository(AppDbContext db) : IAdminUserRepository
{
    public Task<AdminUser?> GetByEmailAsync(string email, CancellationToken ct) =>
        db.AdminUsers.FirstOrDefaultAsync(a => a.Email == email.ToLowerInvariant(), ct);

    public Task<AdminUser?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.AdminUsers.FirstOrDefaultAsync(a => a.Id == id, ct);

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct) =>
        db.AdminUsers.AnyAsync(a => a.Email == email.ToLowerInvariant(), ct);

    public void Add(AdminUser adminUser) => db.AdminUsers.Add(adminUser);
}
