using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Domain.Admins;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AutoHelper.Infrastructure.Persistence;

/// <summary>
/// Seeds a default superadmin account on application startup if one does not exist.
/// </summary>
public static class AdminSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();

        var settings = scope.ServiceProvider.GetRequiredService<IOptions<AdminSeederSettings>>().Value;
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        if (string.IsNullOrWhiteSpace(settings.SuperAdminEmail) ||
            string.IsNullOrWhiteSpace(settings.SuperAdminPassword))
        {
            logger.LogWarning("AdminSeeder: SuperAdminEmail or SuperAdminPassword not configured. Skipping seed.");
            return;
        }

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var email = settings.SuperAdminEmail.Trim().ToLowerInvariant();
        var exists = await db.AdminUsers.AnyAsync(a => a.Email == email);

        if (exists)
            return;

        var hash = passwordHasher.Hash(settings.SuperAdminPassword);
        var superAdmin = AdminUser.Create(email, hash, AdminRole.SuperAdmin);

        db.AdminUsers.Add(superAdmin);
        await db.SaveChangesAsync();

        logger.LogInformation("AdminSeeder: SuperAdmin account created for {Email}", email);
    }
}
