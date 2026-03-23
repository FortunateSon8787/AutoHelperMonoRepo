using AutoHelper.Api.Extensions;
using AutoHelper.Api.Features.Auth;
using AutoHelper.Api.Middleware;
using AutoHelper.Infrastructure.Persistence;
using Scalar.AspNetCore;
using Serilog;

// Bootstrap logger — captures startup failures before full Serilog is configured
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Serilog from appsettings.json
    builder.Host.UseSerilog((ctx, cfg) =>
        cfg.ReadFrom.Configuration(ctx.Configuration));

    builder.AddServices();

    var app = builder.Build();

    // ── Database migrations ───────────────────────────────────────────────────
    // Controlled by Database:AutoMigrateOnStartup in appsettings.json.
    // Defaults: true in Development, false in Production.
    await DatabaseMigrator.MigrateAsync(app.Services);

    // ── Middleware pipeline ───────────────────────────────────────────────────
    app.UseExceptionHandler();

    // 1. Correlation ID must run first so every subsequent log line carries the ID
    app.UseMiddleware<CorrelationIdMiddleware>();

    // 2. Structured HTTP request/response logging (replaces UseSerilogRequestLogging)
    app.UseMiddleware<HttpLoggingMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        // Scalar UI at /scalar/v1
        app.MapScalarApiReference();
    }

    app.UseHttpsRedirection();
    app.UseCors();
    app.UseAuthentication();
    app.UseAuthorization();

    // Health check endpoint
    app.MapHealthChecks("/health");

    // Feature endpoints
    app.MapAuthEndpoints();

    await app.RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    return 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}

return 0;
