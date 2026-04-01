using Amazon.S3;
using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Application.Features.Partners.PartnerSearch;
using AutoHelper.Infrastructure.Ai;
using AutoHelper.Infrastructure.Common;
using AutoHelper.Infrastructure.ExternalServices;
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

        // Admin seeder
        services.Configure<AdminSeederSettings>(configuration.GetSection(AdminSeederSettings.SectionName));

        // Repositories
        services.AddScoped<IAdminUserRepository, AdminUserRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IVehicleRepository, VehicleRepository>();
        services.AddScoped<IServiceRecordRepository, ServiceRecordRepository>();
        services.AddScoped<IPartnerRepository, PartnerRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IAdCampaignRepository, AdCampaignRepository>();
        services.AddScoped<IChatRepository, ChatRepository>();
        services.AddScoped<IInvalidChatRequestRepository, InvalidChatRequestRepository>();
        services.AddScoped<ISubscriptionPlanConfigRepository, SubscriptionPlanConfigRepository>();

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
                ForcePathStyle = true // required for MinIO and Cloudflare R2
            }));

        // LLM Provider (OpenAI)
        services.Configure<LlmSettings>(configuration.GetSection(LlmSettings.SectionName));
        services.AddScoped<ILlmProvider, OpenAiLlmProvider>();
        services.AddScoped<ILlmModelSelector, LlmModelSelector>();
        services.AddScoped<IMarketPriceGateway, LlmMarketPriceGateway>();

        // Google Places API (New)
        services.Configure<GooglePlacesSettings>(configuration.GetSection(GooglePlacesSettings.SectionName));
        services.AddHttpClient<IGooglePlacesService, GooglePlacesService>();

        // Partner search (own DB + Google Places fallback)
        services.AddScoped<IPartnerSearchService, PartnerSearchService>();

        // Select storage implementation based on configured provider.
        // Both MinIO and R2 are S3-compatible; the provider flag controls
        // URL generation and future provider-specific behaviour.
        if (storageSettings.Provider.Equals("R2", StringComparison.OrdinalIgnoreCase))
            services.AddScoped<IStorageService, R2StorageService>();
        else
            services.AddScoped<IStorageService, S3StorageService>();

        return services;
    }
}
