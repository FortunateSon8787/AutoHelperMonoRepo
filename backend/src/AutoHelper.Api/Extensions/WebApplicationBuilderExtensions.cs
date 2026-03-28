using System.Text;
using AutoHelper.Api.Common;
using AutoHelper.Application;
using AutoHelper.Infrastructure;
using AutoHelper.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace AutoHelper.Api.Extensions;

public static class WebApplicationBuilderExtensions
{
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

        // Auth — JWT Bearer with symmetric signing key from configuration
        var jwtSecret = configuration[$"{JwtSettings.SectionName}:Secret"]
            ?? throw new InvalidOperationException("JWT Secret is not configured.");

        var jwtIssuer = configuration[$"{JwtSettings.SectionName}:Issuer"] ?? "autohelper-api";
        var jwtAudience = configuration[$"{JwtSettings.SectionName}:Audience"] ?? "autohelper-api";

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
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
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("admin", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireRole("admin", "superadmin"));
        });

        // Health checks
        services
            .AddHealthChecks()
            .AddDbContextCheck<AutoHelper.Infrastructure.Persistence.AppDbContext>();

        return builder;
    }
}
