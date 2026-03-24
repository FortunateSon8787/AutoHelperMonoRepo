using AutoHelper.Application.Features.Clients.GetMyProfile;
using AutoHelper.Application.Features.Clients.UpdateMyProfile;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AutoHelper.Api.Features.Clients;

public static class ClientsEndpoints
{
    public static void MapClientsEndpoints(this IEndpointRouteBuilder app)
    {
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
                Title = result.Error
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
                Title = result.Error
            });

        return Results.NoContent();
    }
}
