using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Application.Features.Vehicles;
using AutoHelper.Application.Features.Vehicles.ChangeVehicleStatus;
using AutoHelper.Application.Features.Vehicles.CreateVehicle;
using AutoHelper.Application.Features.Vehicles.GetMyVehicles;
using AutoHelper.Application.Features.Vehicles.GetVehicleById;
using AutoHelper.Application.Features.Vehicles.GetVehicleOwner;
using AutoHelper.Application.Features.Vehicles.UpdateVehicle;
using AutoHelper.Domain.Vehicles;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AutoHelper.Api.Features.Vehicles;

public static class VehiclesEndpoints
{
    public static void MapVehicleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/vehicles").WithTags("Vehicles");

        // ─── Public ───────────────────────────────────────────────────────────
        group.MapGet("/{vin}/owner", GetVehicleOwner)
            .WithSummary("Get the public profile of the owner of a vehicle identified by VIN")
            .Produces<VehicleOwnerResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        // ─── Authenticated ────────────────────────────────────────────────────
        var auth = group.RequireAuthorization();

        auth.MapGet("/", GetMyVehicles)
            .WithSummary("Get all vehicles owned by the currently authenticated customer")
            .Produces<IReadOnlyList<VehicleResponse>>(StatusCodes.Status200OK);

        auth.MapPost("/", CreateVehicle)
            .WithSummary("Create a new vehicle for the currently authenticated customer")
            .Produces<CreateVehicleResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict);

        auth.MapGet("/{id:guid}", GetVehicleById)
            .WithSummary("Get details of a vehicle owned by the currently authenticated customer")
            .Produces<VehicleResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        auth.MapPut("/{id:guid}", UpdateVehicle)
            .WithSummary("Update mutable details (brand, model, year, color, mileage) of an owned vehicle")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        auth.MapPut("/{id:guid}/status", ChangeVehicleStatus)
            .WithSummary("Change the status of an owned vehicle; InRepair requires partnerName, Recycled/Dismantled require a PDF document upload")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .DisableAntiforgery();
    }

    // ─── Handlers ─────────────────────────────────────────────────────────────

    private static async Task<IResult> GetVehicleOwner(
        string vin,
        ISender mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetVehicleOwnerQuery(vin), ct);

        if (result.IsFailure)
            return Results.NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = result.Error
            });

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> GetMyVehicles(ISender mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new GetMyVehiclesQuery(), ct);

        if (result.IsFailure)
            return Results.Problem(statusCode: StatusCodes.Status400BadRequest, title: result.Error);

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> CreateVehicle(
        [FromBody] CreateVehicleCommand command,
        ISender mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);

        if (result.IsFailure)
            return Results.Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: result.Error);

        return Results.Created($"/api/vehicles/{result.Value}", new CreateVehicleResponse(result.Value));
    }

    private static async Task<IResult> GetVehicleById(
        Guid id,
        ISender mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetVehicleByIdQuery(id), ct);

        if (result.IsFailure)
            return Results.NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = result.Error
            });

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> UpdateVehicle(
        Guid id,
        [FromBody] UpdateVehicleRequest request,
        ISender mediator,
        CancellationToken ct)
    {
        var command = new UpdateVehicleCommand(id, request.Brand, request.Model, request.Year, request.Color, request.Mileage);
        var result = await mediator.Send(command, ct);

        if (result.IsFailure)
            return Results.NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = result.Error
            });

        return Results.NoContent();
    }

    private static async Task<IResult> ChangeVehicleStatus(
        Guid id,
        [FromForm] string status,
        [FromForm] string? partnerName,
        IFormFile? document,
        ISender mediator,
        IStorageService storage,
        CancellationToken ct)
    {
        if (!Enum.TryParse<VehicleStatus>(status, ignoreCase: true, out var parsedStatus))
            return Results.Problem(statusCode: StatusCodes.Status400BadRequest, title: $"Unknown status: '{status}'.");

        string? documentUrl = null;

        if (document is not null)
        {
            if (!document.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
                return Results.Problem(statusCode: StatusCodes.Status400BadRequest, title: "Only PDF documents are accepted.");

            const long maxBytes = 10 * 1024 * 1024; // 10 MB
            if (document.Length > maxBytes)
                return Results.Problem(statusCode: StatusCodes.Status400BadRequest, title: "Document must not exceed 10 MB.");

            var fileKey = $"vehicles/documents/{Guid.NewGuid()}.pdf";
            await using var stream = document.OpenReadStream();
            documentUrl = await storage.UploadAsync(stream, fileKey, "application/pdf", ct);
        }

        var command = new ChangeVehicleStatusCommand(id, parsedStatus, partnerName, documentUrl);
        var result = await mediator.Send(command, ct);

        if (result.IsFailure)
        {
            var statusCode = result.Error!.Contains("not found", StringComparison.OrdinalIgnoreCase)
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return Results.Problem(statusCode: statusCode, title: result.Error);
        }

        return Results.NoContent();
    }

    // ─── Request / Response DTOs ──────────────────────────────────────────────

    private sealed record CreateVehicleResponse(Guid VehicleId);

    private sealed record UpdateVehicleRequest(
        string Brand,
        string Model,
        int Year,
        string? Color,
        int Mileage);
}
