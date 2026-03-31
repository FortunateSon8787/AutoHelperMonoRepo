using AutoHelper.Application.Features.Admin.SubscriptionPlans.GetAllPlanConfigs;
using AutoHelper.Application.Features.Clients.ActivateSubscription;
using AutoHelper.Application.Features.Clients.ChangePassword;
using AutoHelper.Application.Features.Clients.GetMyProfile;
using AutoHelper.Application.Features.Clients.GetMySubscription;
using AutoHelper.Application.Features.Clients.TopUpRequests;
using AutoHelper.Application.Features.Clients.UpdateMyProfile;
using AutoHelper.Application.Features.Clients.UploadAvatar;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AutoHelper.Api.Features.Clients;

public static class ClientsEndpoints
{
    public static void MapClientsEndpoints(this IEndpointRouteBuilder app)
    {
        // Public — no auth required
        app.MapGet("/api/subscription-plans", GetAllPlanConfigs)
            .WithTags("Clients")
            .WithSummary("Get all subscription plan configurations (price and quota per plan)")
            .Produces<IReadOnlyList<PlanConfigResponse>>(StatusCodes.Status200OK);

        var group = app.MapGroup("/api/clients").WithTags("Clients").RequireAuthorization();

        group.MapGet("/me", GetMyProfile)
            .WithSummary("Get the profile of the currently authenticated customer")
            .Produces<ClientProfileResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/me", UpdateMyProfile)
            .WithSummary("Update the display name and contacts of the currently authenticated customer")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/me/password", ChangePassword)
            .WithSummary("Change the password of the currently authenticated customer (local auth only)")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/me/avatar", UploadAvatar)
            .WithSummary("Upload or replace the avatar of the currently authenticated customer (JPEG/PNG/WebP, max 5 MB)")
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<AvatarUploadResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .DisableAntiforgery();

        group.MapGet("/me/subscription", GetMySubscription)
            .WithSummary("Get the current subscription status and remaining AI requests")
            .Produces<SubscriptionResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/me/subscription/activate", ActivateSubscription)
            .WithSummary("Activate or upgrade a subscription plan (Normal/Pro/Max)")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/me/subscription/topup", TopUpRequests)
            .WithSummary("Add a one-time top-up of AI requests to the current quota")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    // ─── Handlers ─────────────────────────────────────────────────────────────

    private static async Task<IResult> GetMyProfile(
        ISender mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetMyProfileQuery(), ct);

        if (result.IsFailure)
            return Results.NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = result.Error!.Code,
                Detail = result.Error.Description
            });

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> UpdateMyProfile(
        [FromBody] UpdateMyProfileCommand command,
        ISender mediator,
        CancellationToken ct)
    {
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

    private static async Task<IResult> UploadAvatar(
        IFormFile file,
        ISender mediator,
        CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return Results.Problem(statusCode: StatusCodes.Status400BadRequest, title: "No file provided.");

        await using var stream = file.OpenReadStream();

        var command = new UploadAvatarCommand(
            Content: stream,
            FileName: file.FileName,
            ContentType: file.ContentType);

        var result = await mediator.Send(command, ct);

        if (result.IsFailure)
            return Results.Problem(statusCode: StatusCodes.Status400BadRequest, title: result.Error!.Code, detail: result.Error.Description);

        return Results.Ok(new AvatarUploadResponse(result.Value));
    }

    private static async Task<IResult> ChangePassword(
        [FromBody] ChangePasswordCommand command,
        ISender mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);

        if (result.IsFailure)
        {
            if (result.Error!.Code == Application.Common.AppErrors.Customer.NotFound.Code)
                return Results.NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = result.Error.Code,
                    Detail = result.Error.Description
                });

            return Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: result.Error.Code,
                detail: result.Error.Description);
        }

        return Results.NoContent();
    }

    private static async Task<IResult> GetMySubscription(
        ISender mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetMySubscriptionQuery(), ct);

        if (result.IsFailure)
            return Results.Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: result.Error!.Code,
                detail: result.Error.Description);

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> ActivateSubscription(
        [FromBody] ActivateSubscriptionCommand command,
        ISender mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);

        if (result.IsFailure)
            return Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: result.Error!.Code,
                detail: result.Error.Description);

        return Results.NoContent();
    }

    private static async Task<IResult> TopUpRequests(
        [FromBody] TopUpRequestsCommand command,
        ISender mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);

        if (result.IsFailure)
            return Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: result.Error!.Code,
                detail: result.Error.Description);

        return Results.NoContent();
    }

    private static async Task<IResult> GetAllPlanConfigs(ISender mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new GetAllPlanConfigsQuery(), ct);
        return Results.Ok(result.Value);
    }

    // ─── Response DTOs ────────────────────────────────────────────────────────

    private sealed record AvatarUploadResponse(string AvatarUrl);
}
