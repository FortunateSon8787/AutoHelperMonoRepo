using AutoHelper.Application.Features.Partners;
using AutoHelper.Application.Features.Partners.DeactivatePartner;
using AutoHelper.Application.Features.Partners.GetMyPartnerProfile;
using AutoHelper.Application.Features.Partners.GetPendingPartners;
using AutoHelper.Application.Features.Partners.RegisterPartner;
using AutoHelper.Application.Features.Partners.UpdateMyPartnerProfile;
using AutoHelper.Application.Features.Partners.VerifyPartner;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AutoHelper.Api.Features.Partners;

public static class PartnersEndpoints
{
    public static void MapPartnerEndpoints(this IEndpointRouteBuilder app)
    {
        // ── Partner self-service (requires auth) ──────────────────────────────
        var partnerGroup = app.MapGroup("/api/partners").WithTags("Partners").RequireAuthorization();

        partnerGroup.MapPost("/register", RegisterPartner)
            .WithSummary("Register a new partner profile for the authenticated user")
            .Produces<RegisterPartnerResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict);

        partnerGroup.MapGet("/me", GetMyProfile)
            .WithSummary("Get the profile of the currently authenticated partner")
            .Produces<PartnerResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        partnerGroup.MapPut("/me", UpdateMyProfile)
            .WithSummary("Update the profile of the currently authenticated partner")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        // ── Admin operations ──────────────────────────────────────────────────
        var adminGroup = app.MapGroup("/api/admin/partners").WithTags("Admin: Partners").RequireAuthorization("admin");

        adminGroup.MapGet("/pending", GetPendingPartners)
            .WithSummary("Get all partner profiles awaiting admin verification")
            .Produces<IReadOnlyList<PartnerResponse>>(StatusCodes.Status200OK);

        adminGroup.MapPost("/{id:guid}/verify", VerifyPartner)
            .WithSummary("Verify a partner profile and activate it (admin only)")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        adminGroup.MapPost("/{id:guid}/deactivate", DeactivatePartner)
            .WithSummary("Deactivate a partner profile (admin only)")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    // ─── Partner self-service handlers ────────────────────────────────────────

    private static async Task<IResult> RegisterPartner(
        [FromBody] RegisterPartnerCommand command,
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

        return Results.Created($"/api/partners/me", new RegisterPartnerResponse(result.Value));
    }

    private static async Task<IResult> GetMyProfile(
        ISender mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetMyPartnerProfileQuery(), ct);

        if (result.IsFailure)
            return Results.NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = result.Error
            });

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> UpdateMyProfile(
        [FromBody] UpdateMyPartnerProfileCommand command,
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

    // ─── Admin handlers ───────────────────────────────────────────────────────

    private static async Task<IResult> GetPendingPartners(
        ISender mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetPendingPartnersQuery(), ct);
        return Results.Ok(result.Value);
    }

    private static async Task<IResult> VerifyPartner(
        Guid id,
        ISender mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new VerifyPartnerCommand(id), ct);

        if (result.IsFailure)
            return Results.NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = result.Error
            });

        return Results.NoContent();
    }

    private static async Task<IResult> DeactivatePartner(
        Guid id,
        ISender mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new DeactivatePartnerCommand(id), ct);

        if (result.IsFailure)
            return Results.NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = result.Error
            });

        return Results.NoContent();
    }

    // ─── Response DTOs ────────────────────────────────────────────────────────

    private sealed record RegisterPartnerResponse(Guid PartnerId);
}
