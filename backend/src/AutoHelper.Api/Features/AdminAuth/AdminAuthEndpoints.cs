using AutoHelper.Api.Extensions;
using AutoHelper.Application.Features.AdminAuth.Login;
using AutoHelper.Application.Features.AdminAuth.Logout;
using AutoHelper.Application.Features.AdminAuth.RefreshToken;
using AutoHelper.Infrastructure.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AutoHelper.Api.Features.AdminAuth;

public static class AdminAuthEndpoints
{
    public static void MapAdminAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/auth").WithTags("Admin — Auth");

        group.MapPost("/login", Login)
            .WithSummary("Authenticate admin with email and password, set httpOnly admin auth cookies")
            .RequireRateLimiting(WebApplicationBuilderExtensions.LoginRateLimitPolicy)
            .Produces(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapPost("/refresh", Refresh)
            .WithSummary("Rotate admin tokens using the refresh cookie, set new httpOnly admin auth cookies")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapPost("/logout", Logout)
            .RequireAuthorization(WebApplicationBuilderExtensions.AdminPolicy)
            .WithSummary("Revoke admin refresh token and clear admin auth cookies")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Login(
        [FromBody] LoginAdminCommand command,
        ISender mediator,
        HttpContext httpContext,
        IOptions<JwtSettings> jwtOptions,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);

        if (result.IsFailure)
            return Results.Unauthorized();

        SetAdminCookies(httpContext, result.Value.AccessToken, result.Value.RefreshToken,
            result.Value.ExpiresAt, jwtOptions.Value);

        return Results.Ok();
    }

    private static async Task<IResult> Refresh(
        ISender mediator,
        HttpContext httpContext,
        IOptions<JwtSettings> jwtOptions,
        CancellationToken ct)
    {
        var refreshToken = httpContext.Request.Cookies["adminRefreshToken"];

        if (string.IsNullOrEmpty(refreshToken))
            return Results.Unauthorized();

        var result = await mediator.Send(new RefreshAdminTokenCommand(refreshToken), ct);

        if (result.IsFailure)
            return Results.Unauthorized();

        SetAdminCookies(httpContext, result.Value.AccessToken, result.Value.RefreshToken,
            result.Value.ExpiresAt, jwtOptions.Value);

        return Results.Ok();
    }

    private static async Task<IResult> Logout(
        ISender mediator,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var refreshToken = httpContext.Request.Cookies["adminRefreshToken"];

        if (string.IsNullOrEmpty(refreshToken))
        {
            ClearAdminCookies(httpContext);
            return Results.NoContent();
        }

        var result = await mediator.Send(new LogoutAdminCommand(refreshToken), ct);

        if (result.IsFailure)
            return Results.Problem(
                title: result.Error!.Code,
                detail: result.Error.Description,
                statusCode: StatusCodes.Status404NotFound);

        ClearAdminCookies(httpContext);
        return Results.NoContent();
    }

    // ─── Cookie Helpers ───────────────────────────────────────────────────────

    private static void SetAdminCookies(
        HttpContext httpContext,
        string accessToken,
        string refreshToken,
        DateTime refreshTokenExpiresAt,
        JwtSettings settings)
    {
        httpContext.Response.Cookies.Append("adminAccessToken", accessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddMinutes(settings.AdminAccessTokenExpiryMinutes)
        });

        httpContext.Response.Cookies.Append("adminRefreshToken", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = new DateTimeOffset(refreshTokenExpiresAt, TimeSpan.Zero)
        });
    }

    private static void ClearAdminCookies(HttpContext httpContext)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict
        };

        httpContext.Response.Cookies.Delete("adminAccessToken", cookieOptions);
        httpContext.Response.Cookies.Delete("adminRefreshToken", cookieOptions);
    }
}
