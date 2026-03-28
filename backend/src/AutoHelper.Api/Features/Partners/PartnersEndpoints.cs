using AutoHelper.Application.Features.Partners;
using AutoHelper.Application.Features.Partners.DeactivatePartner;
using AutoHelper.Application.Features.Partners.GetMyPartnerProfile;
using AutoHelper.Application.Features.Partners.GetPendingPartners;
using AutoHelper.Application.Features.Partners.RegisterPartner;
using AutoHelper.Application.Features.Partners.UpdateMyPartnerProfile;
using AutoHelper.Application.Features.Partners.VerifyPartner;
using AutoHelper.Application.Features.Reviews.CreateReview;
using AutoHelper.Application.Features.Reviews.GetPartnerReviews;
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

        partnerGroup.MapPost("/{partnerId:guid}/reviews", CreateReview)
            .WithSummary("Submit a review for a partner (requires verifiable interaction)")
            .Produces<CreateReviewResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        // ── Public review listing ─────────────────────────────────────────────
        var publicGroup = app.MapGroup("/api/partners").WithTags("Partners");

        publicGroup.MapGet("/{partnerId:guid}/reviews", GetPartnerReviews)
            .WithSummary("Get all reviews for a partner (public, no auth required)")
            .Produces<IReadOnlyList<ReviewResponse>>(StatusCodes.Status200OK);

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

    // ─── Review handlers ──────────────────────────────────────────────────────

    private static async Task<IResult> CreateReview(
        Guid partnerId,
        [FromBody] CreateReviewRequest request,
        ISender mediator,
        CancellationToken ct)
    {
        var command = new CreateReviewCommand(
            PartnerId: partnerId,
            Rating: request.Rating,
            Comment: request.Comment,
            Basis: request.Basis,
            InteractionReferenceId: request.InteractionReferenceId);

        var result = await mediator.Send(command, ct);

        if (result.IsFailure)
        {
            if (result.Error!.Contains("not found"))
                return Results.NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = result.Error
                });

            return Results.Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = result.Error
            });
        }

        return Results.Created(
            $"/api/partners/{partnerId}/reviews/{result.Value}",
            new CreateReviewResponse(result.Value));
    }

    private static async Task<IResult> GetPartnerReviews(
        Guid partnerId,
        ISender mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetPartnerReviewsQuery(partnerId), ct);
        return Results.Ok(result.Value);
    }

    // ─── Response DTOs ────────────────────────────────────────────────────────

    private sealed record RegisterPartnerResponse(Guid PartnerId);
    private sealed record CreateReviewResponse(Guid ReviewId);
    private sealed record CreateReviewRequest(
        int Rating,
        string Comment,
        string Basis,
        Guid InteractionReferenceId);
}
