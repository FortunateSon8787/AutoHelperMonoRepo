using AutoHelper.Application.Features.Auth.Login;
using AutoHelper.Application.Features.Auth.Logout;
using AutoHelper.Application.Features.Auth.Register;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using RefreshTokenCommand = AutoHelper.Application.Features.Auth.RefreshToken.RefreshTokenCommand;

namespace AutoHelper.Api.Features.Auth;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/register", Register)
            .WithSummary("Register a new customer with email and password")
            .Produces<RegisterResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/login", Login)
            .WithSummary("Authenticate with email and password, receive JWT tokens")
            .Produces<TokenResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapPost("/refresh", Refresh)
            .WithSummary("Exchange a refresh token for a new access + refresh token pair (token rotation)")
            .Produces<TokenResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapPost("/logout", Logout)
            .WithSummary("Revoke the current refresh token, ending the session")
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
                Title = result.Error
            });

        return Results.Created($"/api/customers/{result.Value}", new RegisterResponse(result.Value));
    }

    private static async Task<IResult> Login(
        [FromBody] LoginCommand command,
        ISender mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);

        if (result.IsFailure)
            return Results.Unauthorized();

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> Refresh(
        [FromBody] RefreshTokenCommand command,
        ISender mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);

        if (result.IsFailure)
            return Results.Unauthorized();

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> Logout(
        [FromBody] LogoutCommand command,
        ISender mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);

        if (result.IsFailure)
            return Results.NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = result.Error
            });

        return Results.NoContent();
    }

    // ─── Response DTOs ────────────────────────────────────────────────────────

    private sealed record RegisterResponse(Guid CustomerId);
}
