using AutoHelper.Application.Features.AdminAuth.Login;
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
            .Produces(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapPost("/logout", (HttpContext ctx) =>
            {
                ClearAdminCookies(ctx);
                return Results.NoContent();
            })
            .WithSummary("Clear admin auth cookies")
            .Produces(StatusCodes.Status204NoContent);
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
            Expires = DateTimeOffset.UtcNow.AddMinutes(settings.AccessTokenExpiryMinutes)
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
        httpContext.Response.Cookies.Delete("adminAccessToken");
        httpContext.Response.Cookies.Delete("adminRefreshToken");
    }
}
