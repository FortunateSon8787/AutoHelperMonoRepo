using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Application.Features.ServiceRecords;
using AutoHelper.Application.Features.ServiceRecords.CreateServiceRecord;
using AutoHelper.Application.Features.ServiceRecords.DeleteServiceRecord;
using AutoHelper.Application.Features.ServiceRecords.GetPublicServiceRecords;
using AutoHelper.Application.Features.ServiceRecords.GetServiceRecordById;
using AutoHelper.Application.Features.ServiceRecords.GetServiceRecords;
using AutoHelper.Application.Features.ServiceRecords.UpdateServiceRecord;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AutoHelper.Api.Features.ServiceRecords;

public static class ServiceRecordsEndpoints
{
    public static void MapServiceRecordEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api").WithTags("ServiceRecords");

        // ─── Public ───────────────────────────────────────────────────────────

        group.MapGet("/vehicles/{vin}/service-records", GetPublicServiceRecords)
            .WithSummary("Get the public service history of a vehicle by VIN")
            .Produces<IReadOnlyList<ServiceRecordResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        // ─── Authenticated ────────────────────────────────────────────────────

        var auth = group.RequireAuthorization();

        auth.MapGet("/vehicles/{vehicleId:guid}/service-records", GetServiceRecords)
            .WithSummary("Get all service records for an owned vehicle")
            .Produces<IReadOnlyList<ServiceRecordResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        auth.MapGet("/service-records/{id:guid}", GetServiceRecordById)
            .WithSummary("Get a single service record by ID")
            .Produces<ServiceRecordResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        auth.MapPost("/vehicles/{vehicleId:guid}/service-records", CreateServiceRecord)
            .WithSummary("Create a service record for an owned vehicle. Upload the PDF first via POST /service-records/document, then pass the returned DocumentUrl here.")
            .Produces<CreateServiceRecordResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .DisableAntiforgery();

        auth.MapPut("/service-records/{id:guid}", UpdateServiceRecord)
            .WithSummary("Update mutable fields of a service record (document is immutable)")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        auth.MapDelete("/service-records/{id:guid}", DeleteServiceRecord)
            .WithSummary("Soft-delete a service record")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        // Upload PDF document — returns the storage URL to be passed into CreateServiceRecord
        auth.MapPost("/service-records/document", UploadServiceDocument)
            .WithSummary("Upload a PDF work order document. Returns the DocumentUrl to use when creating a service record.")
            .Produces<UploadDocumentResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .DisableAntiforgery();
    }

    // ─── Handlers ─────────────────────────────────────────────────────────────

    private static async Task<IResult> GetPublicServiceRecords(
        string vin,
        ISender mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetPublicServiceRecordsQuery(vin), ct);

        if (result.IsFailure)
            return Results.NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = result.Error
            });

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> GetServiceRecords(
        Guid vehicleId,
        ISender mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetServiceRecordsQuery(vehicleId), ct);

        if (result.IsFailure)
            return Results.NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = result.Error
            });

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> GetServiceRecordById(
        Guid id,
        ISender mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetServiceRecordByIdQuery(id), ct);

        if (result.IsFailure)
            return Results.NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = result.Error
            });

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> CreateServiceRecord(
        Guid vehicleId,
        [FromBody] CreateServiceRecordRequest request,
        ISender mediator,
        CancellationToken ct)
    {
        var command = new CreateServiceRecordCommand(
            vehicleId,
            request.Title,
            request.Description,
            request.PerformedAt,
            request.Cost,
            request.ExecutorName,
            request.ExecutorContacts,
            request.Operations,
            request.DocumentUrl);

        var result = await mediator.Send(command, ct);

        if (result.IsFailure)
        {
            var statusCode = result.Error!.Contains("not found", StringComparison.OrdinalIgnoreCase)
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return Results.Problem(statusCode: statusCode, title: result.Error);
        }

        return Results.Created(
            $"/api/service-records/{result.Value}",
            new CreateServiceRecordResponse(result.Value));
    }

    private static async Task<IResult> UpdateServiceRecord(
        Guid id,
        [FromBody] UpdateServiceRecordRequest request,
        ISender mediator,
        CancellationToken ct)
    {
        var command = new UpdateServiceRecordCommand(
            id,
            request.Title,
            request.Description,
            request.PerformedAt,
            request.Cost,
            request.ExecutorName,
            request.ExecutorContacts,
            request.Operations);

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

    private static async Task<IResult> DeleteServiceRecord(
        Guid id,
        ISender mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteServiceRecordCommand(id), ct);

        if (result.IsFailure)
        {
            var statusCode = result.Error!.Contains("not found", StringComparison.OrdinalIgnoreCase)
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return Results.Problem(statusCode: statusCode, title: result.Error);
        }

        return Results.NoContent();
    }

    private static async Task<IResult> UploadServiceDocument(
        IFormFile document,
        IStorageService storage,
        CancellationToken ct)
    {
        if (document is null)
            return Results.Problem(statusCode: StatusCodes.Status400BadRequest, title: "No file uploaded.");

        if (!document.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
            return Results.Problem(statusCode: StatusCodes.Status400BadRequest, title: "Only PDF documents are accepted.");

        const long maxBytes = 10 * 1024 * 1024; // 10 MB
        if (document.Length > maxBytes)
            return Results.Problem(statusCode: StatusCodes.Status400BadRequest, title: "Document must not exceed 10 MB.");

        // Use a UUID-based key so filenames never conflict
        var fileKey = $"service-records/documents/{Guid.NewGuid()}.pdf";
        await using var stream = document.OpenReadStream();
        var documentUrl = await storage.UploadAsync(stream, fileKey, "application/pdf", ct);

        return Results.Ok(new UploadDocumentResponse(documentUrl));
    }

    // ─── Request / Response DTOs ──────────────────────────────────────────────

    private sealed record CreateServiceRecordRequest(
        string Title,
        string Description,
        DateTime PerformedAt,
        decimal Cost,
        string ExecutorName,
        string? ExecutorContacts,
        List<string> Operations,
        string DocumentUrl);

    private sealed record CreateServiceRecordResponse(Guid ServiceRecordId);

    private sealed record UpdateServiceRecordRequest(
        string Title,
        string Description,
        DateTime PerformedAt,
        decimal Cost,
        string ExecutorName,
        string? ExecutorContacts,
        List<string> Operations);

    private sealed record UploadDocumentResponse(string DocumentUrl);
}
