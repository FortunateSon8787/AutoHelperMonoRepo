using AutoHelper.Application.Features.AdCampaigns;
using AutoHelper.Application.Features.AdCampaigns.CreateAdCampaign;
using AutoHelper.Application.Features.AdCampaigns.DeleteAdCampaign;
using AutoHelper.Application.Features.AdCampaigns.GetActiveAds;
using AutoHelper.Application.Features.AdCampaigns.GetMyCampaigns;
using AutoHelper.Application.Features.AdCampaigns.UpdateAdCampaign;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AutoHelper.Api.Features.AdCampaigns;

public static class AdCampaignEndpoints
{
    public static void MapAdCampaignEndpoints(this IEndpointRouteBuilder app)
    {
        // ── Partner: ad campaign management (requires auth) ───────────────────
        var partnerGroup = app.MapGroup("/api/ad-campaigns")
            .WithTags("Ad Campaigns")
            .RequireAuthorization();

        partnerGroup.MapGet("/my", GetMyCampaigns)
            .WithSummary("Get all ad campaigns for the authenticated partner")
            .Produces<IReadOnlyList<AdCampaignResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        partnerGroup.MapPost("/", CreateCampaign)
            .WithSummary("Create a new ad campaign (partner only)")
            .Produces<CreateAdCampaignResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        partnerGroup.MapPut("/{id:guid}", UpdateCampaign)
            .WithSummary("Update an existing ad campaign")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        partnerGroup.MapDelete("/{id:guid}", DeleteCampaign)
            .WithSummary("Soft-delete an ad campaign")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        // ── Public: display ads (no auth required) ────────────────────────────
        var publicGroup = app.MapGroup("/api/ads").WithTags("Ad Campaigns");

        publicGroup.MapGet("/", GetActiveAds)
            .WithSummary("Get active ad campaigns for display (with targeting and rotation)")
            .Produces<IReadOnlyList<AdCampaignResponse>>(StatusCodes.Status200OK);
    }

    // ─── Partner handlers ─────────────────────────────────────────────────────

    private static async Task<IResult> GetMyCampaigns(
        ISender mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetMyCampaignsQuery(), ct);

        if (result.IsFailure)
            return Results.NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = result.Error!.Code,
                Detail = result.Error.Description
            });

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> CreateCampaign(
        [FromBody] CreateAdCampaignCommand command,
        ISender mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);

        if (result.IsFailure)
            return Results.BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = result.Error!.Code,
                Detail = result.Error.Description
            });

        return Results.Created($"/api/ad-campaigns/{result.Value}", new CreateAdCampaignResponse(result.Value));
    }

    private static async Task<IResult> UpdateCampaign(
        Guid id,
        [FromBody] UpdateAdCampaignRequest request,
        ISender mediator,
        CancellationToken ct)
    {
        var command = new UpdateAdCampaignCommand(
            Id: id,
            Type: request.Type,
            TargetCategory: request.TargetCategory,
            Content: request.Content,
            StartsAt: request.StartsAt,
            EndsAt: request.EndsAt,
            ShowToAnonymous: request.ShowToAnonymous);

        var result = await mediator.Send(command, ct);

        if (result.IsFailure)
            return Results.NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = result.Error!.Code,
                Detail = result.Error.Description
            });

        return Results.NoContent();
    }

    private static async Task<IResult> DeleteCampaign(
        Guid id,
        ISender mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteAdCampaignCommand(id), ct);

        if (result.IsFailure)
            return Results.NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = result.Error!.Code,
                Detail = result.Error.Description
            });

        return Results.NoContent();
    }

    // ─── Public handlers ──────────────────────────────────────────────────────

    private static async Task<IResult> GetActiveAds(
        [FromQuery] bool isAuthenticated,
        [FromQuery] bool isPartner,
        [FromQuery] string? targetCategory,
        ISender mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(
            new GetActiveAdsQuery(isAuthenticated, isPartner, targetCategory), ct);

        if (result.IsFailure)
            return Results.BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = result.Error!.Code,
                Detail = result.Error.Description
            });

        return Results.Ok(result.Value);
    }

    // ─── Response / Request DTOs ──────────────────────────────────────────────

    private sealed record CreateAdCampaignResponse(Guid CampaignId);

    private sealed record UpdateAdCampaignRequest(
        string Type,
        string TargetCategory,
        string Content,
        DateTime StartsAt,
        DateTime EndsAt,
        bool ShowToAnonymous);
}
