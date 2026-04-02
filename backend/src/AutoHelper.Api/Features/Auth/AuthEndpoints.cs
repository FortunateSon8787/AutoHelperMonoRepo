using AutoHelper.Api.Extensions;
using AutoHelper.Application.Features.Auth.Login;
using AutoHelper.Application.Features.Auth.Logout;
using AutoHelper.Application.Features.Auth.Register;
using AutoHelper.Infrastructure.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RefreshTokenCommand = AutoHelper.Application.Features.Auth.RefreshToken.RefreshTokenCommand;

namespace AutoHelper.Api.Features.Auth;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/register", Register)
            .WithSummary("Register a new customer with email and password")
            .RequireRateLimiting(WebApplicationBuilderExtensions.LoginRateLimitPolicy)
            .Produces<RegisterResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/login", Login)
            .WithSummary("Authenticate with email and password, set httpOnly auth cookies")
            .RequireRateLimiting(WebApplicationBuilderExtensions.LoginRateLimitPolicy)
            .Produces(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapPost("/refresh", Refresh)
            .WithSummary("Rotate tokens using the refresh cookie, set new httpOnly auth cookies")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapPost("/logout", Logout)
            .WithSummary("Revoke the current refresh token and clear auth cookies")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    // ─── Handlers ─────────────────────────────────────────────────────────────

    private static async Task<IResult> Register(
        [FromBody] RegisterCustomerCommand command,
        ISender mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);

        if (result.IsFailure)
            return Results.Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = result.Error!.Code,
                Detail = result.Error.Description
            });

        return Results.Created($"/api/customers/{result.Value}", new RegisterResponse(result.Value));
    }

    private static async Task<IResult> Login(
        [FromBody] LoginCommand command,
        ISender mediator,
        HttpContext httpContext,
        IOptions<JwtSettings> jwtOptions,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);

        if (result.IsFailure)
            return Results.Unauthorized();

        SetAuthCookies(httpContext, result.Value, jwtOptions.Value);
        return Results.Ok();
    }

    private static async Task<IResult> Refresh(
        ISender mediator,
        HttpContext httpContext,
        IOptions<JwtSettings> jwtOptions,
        CancellationToken ct)
    {
        var refreshToken = httpContext.Request.Cookies["refreshToken"];

        if (string.IsNullOrEmpty(refreshToken))
            return Results.Unauthorized();

        var result = await mediator.Send(new RefreshTokenCommand(refreshToken), ct);

        if (result.IsFailure)
            return Results.Unauthorized();

        SetAuthCookies(httpContext, result.Value, jwtOptions.Value);
        return Results.Ok();
    }

    private static async Task<IResult> Logout(
        ISender mediator,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var refreshToken = httpContext.Request.Cookies["refreshToken"];

        if (string.IsNullOrEmpty(refreshToken))
            return Results.NoContent();

        var result = await mediator.Send(new LogoutCommand(refreshToken), ct);

        if (result.IsFailure)
            return Results.NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = result.Error!.Code,
                Detail = result.Error.Description
            });

        ClearAuthCookies(httpContext);
        return Results.NoContent();
    }

    // ─── Cookie Helpers ───────────────────────────────────────────────────────

    private static void SetAuthCookies(HttpContext httpContext, TokenResponse tokens, JwtSettings settings)
    {
        httpContext.Response.Cookies.Append("accessToken", tokens.AccessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddMinutes(settings.AccessTokenExpiryMinutes)
        });

        httpContext.Response.Cookies.Append("refreshToken", tokens.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = new DateTimeOffset(tokens.ExpiresAt, TimeSpan.Zero)
        });
    }

    private static void ClearAuthCookies(HttpContext httpContext)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict
        };

        httpContext.Response.Cookies.Delete("accessToken", cookieOptions);
        httpContext.Response.Cookies.Delete("refreshToken", cookieOptions);
    }

    // ─── Response DTOs ────────────────────────────────────────────────────────

    private sealed record RegisterResponse(Guid CustomerId);
}
