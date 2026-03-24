using Amazon.S3;
using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Infrastructure.Common;
using AutoHelper.Infrastructure.Persistence;
using AutoHelper.Infrastructure.Persistence.Repositories;
using AutoHelper.Infrastructure.Security;
using AutoHelper.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AutoHelper.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        var connectionString = configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException("Connection string 'Database' is not configured.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());

        // Current user
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();

        // Repositories
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IVehicleRepository, VehicleRepository>();

        // Security
        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        // JWT
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.AddSingleton<IJwtTokenService, JwtTokenService>();

        // Storage (S3/MinIO)
        services.Configure<StorageSettings>(configuration.GetSection(StorageSettings.SectionName));

        var storageSettings = configuration.GetSection(StorageSettings.SectionName).Get<StorageSettings>()
            ?? new StorageSettings();

        services.AddSingleton<IAmazonS3>(_ => new AmazonS3Client(
            awsAccessKeyId: storageSettings.AccessKey,
            awsSecretAccessKey: storageSettings.SecretKey,
            clientConfig: new AmazonS3Config
            {
                ServiceURL = storageSettings.ServiceUrl,
                ForcePathStyle = true // required for MinIO
            }));

        services.AddScoped<IStorageService, S3StorageService>();

        return services;
    }
}
