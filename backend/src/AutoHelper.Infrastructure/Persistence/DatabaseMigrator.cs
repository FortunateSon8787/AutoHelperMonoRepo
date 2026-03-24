using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AutoHelper.Infrastructure.Persistence;

/// <summary>
/// Applies pending EF Core migrations on application startup.
///
/// Controlled by the feature flag <c>Database:AutoMigrateOnStartup</c> (bool).
///
/// Behaviour:
///   <b>true</b>  — pending migrations are detected and applied automatically.
///                 The application will not start if migrations fail.
///   <b>false</b> — migrations are skipped (apply manually: dotnet ef database update).
///
/// Defaults:
///   Development  → true   (set in appsettings.Development.json)
///   Production   → false  (set in appsettings.json)
/// </summary>
public static class DatabaseMigrator
{
    public static async Task MigrateAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        bool autoMigrate;
        try
        {
            var config = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();
            autoMigrate = bool.TryParse(config["Database:AutoMigrateOnStartup"], out var val) && val;
        }
        catch
        {
            autoMigrate = false;
        }

        if (!autoMigrate)
        {
            logger.LogInformation(
                "Auto-migrations are disabled (Database:AutoMigrateOnStartup = false). " +
                "Run 'dotnet ef database update' to apply pending migrations manually.");
            return;
        }

        logger.LogInformation("Auto-migrations enabled. Checking for pending migrations...");

        try
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var pendingMigrations = (await db.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();

            if (pendingMigrations.Count == 0)
            {
                logger.LogInformation("Database is up to date. No pending migrations found.");
                return;
            }

            logger.LogInformation(
                "Found {PendingCount} pending migration(s): {Migrations}",
                pendingMigrations.Count,
                pendingMigrations);

            await db.Database.MigrateAsync(cancellationToken);

            var appliedMigrations = (await db.Database.GetAppliedMigrationsAsync(cancellationToken)).ToList();

            logger.LogInformation(
                "Successfully applied {PendingCount} migration(s). " +
                "Total applied migrations: {TotalApplied}.",
                pendingMigrations.Count,
                appliedMigrations.Count);
        }
        catch (Exception ex)
        {
            logger.LogCritical(
                ex,
                "Failed to apply database migrations. " +
                "The application cannot start in a safe state. " +
                "Check the database connection and migration scripts. " +
                "To skip auto-migrations set Database:AutoMigrateOnStartup = false.");

            // Re-throw so the host startup fails loudly with a clear message
            throw new InvalidOperationException(
                "Database migration failed on startup. See inner exception for details.", ex);
        }
    }
}
