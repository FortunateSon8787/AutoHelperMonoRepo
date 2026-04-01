using AutoHelper.Domain.Admins;

namespace AutoHelper.Application.Common.Interfaces;

public interface IAdminUserRepository
{
    Task<AdminUser?> GetByEmailAsync(string email, CancellationToken ct);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct);
    void Add(AdminUser adminUser);
}
