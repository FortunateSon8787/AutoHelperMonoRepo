using System.Text;
using AutoHelper.Api.Common;
using AutoHelper.Application;
using AutoHelper.Infrastructure;
using AutoHelper.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using System.Threading.RateLimiting;

namespace AutoHelper.Api.Extensions;

public static class WebApplicationBuilderExtensions
{
    internal const string ClientScheme = "ClientJwt";
    internal const string AdminScheme = "AdminJwt";
    internal const string LoginRateLimitPolicy = "login";

    public static WebApplicationBuilder AddServices(this WebApplicationBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;

        // Application and Infrastructure layers
        services.AddApplicationServices(configuration);
        services.AddInfrastructureServices(configuration);

        // API-level services
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        services.AddOpenApi();

        // CORS — allow the Next.js frontend
        var allowedOrigins = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? [];

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
                policy
                    .WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials());
        });

        // ── JWT secrets ───────────────────────────────────────────────────────
        var jwtSecret = configuration[$"{JwtSettings.SectionName}:Secret"]
            ?? throw new InvalidOperationException("JWT Secret is not configured.");

        var adminSecret = configuration[$"{JwtSettings.SectionName}:AdminSecret"];
        var effectiveAdminSecret = string.IsNullOrWhiteSpace(adminSecret) ? jwtSecret : adminSecret;

        var jwtIssuer = configuration[$"{JwtSettings.SectionName}:Issuer"] ?? "autohelper-api";
        var jwtAudience = configuration[$"{JwtSettings.SectionName}:Audience"] ?? "autohelper-api";

        // ── Authentication: two separate schemes ──────────────────────────────
        services
            .AddAuthentication(options =>
            {
                // Default scheme — client JWT (used for non-admin endpoints)
                options.DefaultAuthenticateScheme = ClientScheme;
                options.DefaultChallengeScheme = ClientScheme;
            })
            .AddJwtBearer(ClientScheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                    ValidateIssuer = true,
                    ValidIssuer = jwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = jwtAudience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var token = context.Request.Cookies["accessToken"];
                        if (!string.IsNullOrEmpty(token))
                            context.Token = token;

                        return Task.CompletedTask;
                    }
                };
            })
            .AddJwtBearer(AdminScheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(effectiveAdminSecret)),
                    ValidateIssuer = true,
                    ValidIssuer = jwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = jwtAudience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var token = context.Request.Cookies["adminAccessToken"];
                        if (!string.IsNullOrEmpty(token))
                            context.Token = token;

                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("admin", policy =>
                policy
                    .AddAuthenticationSchemes(AdminScheme)
                    .RequireAuthenticatedUser()
                    .RequireRole("admin", "superadmin"));
        });

        // ── Rate limiting ─────────────────────────────────────────────────────
        services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter(LoginRateLimitPolicy, limiterOptions =>
            {
                limiterOptions.Window = TimeSpan.FromMinutes(1);
                limiterOptions.PermitLimit = 10;
                limiterOptions.QueueLimit = 0;
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            });

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });

        // Health checks
        services
            .AddHealthChecks()
            .AddDbContextCheck<AutoHelper.Infrastructure.Persistence.AppDbContext>();

        return builder;
    }
}
