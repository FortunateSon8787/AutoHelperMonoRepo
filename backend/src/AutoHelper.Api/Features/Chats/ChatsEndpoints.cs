using AutoHelper.Application.Common;
using AutoHelper.Application.Features.Chats.CreateChat;
using AutoHelper.Application.Features.Chats.DeleteChat;
using AutoHelper.Application.Features.Chats.GetChatMessages;
using AutoHelper.Application.Features.Chats.GetMyChats;
using AutoHelper.Application.Features.Chats.SendMessage;
using AutoHelper.Domain.Chats;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PartnerAdviceInput = AutoHelper.Domain.Chats.PartnerAdviceInput;

namespace AutoHelper.Api.Features.Chats;

public static class ChatsEndpoints
{
    public static void MapChatEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/chats").WithTags("Chats").RequireAuthorization();

        group.MapGet("/", GetMyChats)
            .WithSummary("Get paginated chat sessions for the authenticated customer");

        group.MapPost("/", CreateChat)
            .WithSummary("Create a new AI chat session (optionally linked to a vehicle). " +
                         "For FaultHelp mode, diagnostics_input is required and the first assistant " +
                         "reply is returned immediately.")
            .Produces<CreateChatApiResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapGet("/{chatId:guid}/messages", GetChatMessages)
            .WithSummary("Get the full message history for a chat session")
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{chatId:guid}/messages", SendMessage)
            .WithSummary("Send a message and receive the LLM response")
            .Produces<SendMessageApiResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{chatId:guid}", DeleteChat)
            .WithSummary("Soft-delete a chat session")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    // ─── Handlers ─────────────────────────────────────────────────────────────

    private static async Task<IResult> GetMyChats(
        ISender mediator,
        CancellationToken ct,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await mediator.Send(new GetMyChatsQuery(page, pageSize), ct);

        if (result.IsFailure)
            return Results.Unauthorized();

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> CreateChat(
        [FromBody] CreateChatApiRequest request,
        ISender mediator,
        CancellationToken ct)
    {
        DiagnosticsInput? diagnosticsInput = request.DiagnosticsInput is not null
            ? new DiagnosticsInput
            {
                Symptoms = request.DiagnosticsInput.Symptoms,
                RecentEvents = request.DiagnosticsInput.RecentEvents,
                PreviousIssues = request.DiagnosticsInput.PreviousIssues
            }
            : null;

        WorkClarificationInput? workClarificationInput = request.WorkClarificationInput is not null
            ? new WorkClarificationInput
            {
                WorksPerformed = request.WorkClarificationInput.WorksPerformed,
                WorkReason = request.WorkClarificationInput.WorkReason,
                LaborCost = request.WorkClarificationInput.LaborCost,
                PartsCost = request.WorkClarificationInput.PartsCost,
                Guarantees = request.WorkClarificationInput.Guarantees
            }
            : null;

        PartnerAdviceInput? partnerAdviceInput = request.PartnerAdviceInput is not null
            ? new PartnerAdviceInput
            {
                Request = request.PartnerAdviceInput.Request,
                Lat = request.PartnerAdviceInput.Lat,
                Lng = request.PartnerAdviceInput.Lng,
                Urgency = request.PartnerAdviceInput.Urgency
            }
            : null;

        var command = new CreateChatCommand(
            Mode: request.Mode,
            Title: request.Title,
            VehicleId: request.VehicleId,
            DiagnosticsInput: diagnosticsInput,
            WorkClarificationInput: workClarificationInput,
            PartnerAdviceInput: partnerAdviceInput,
            Locale: request.Locale ?? "ru");

        var result = await mediator.Send(command, ct);

        if (result.IsFailure)
        {
            if (result.Error!.Code == AppErrors.Auth.NotAuthenticated.Code)
                return Results.Unauthorized();

            return Results.Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: result.Error.Code,
                detail: result.Error.Description);
        }

        return Results.Created(
            $"/api/chats/{result.Value.ChatId}",
            new CreateChatApiResponse(result.Value.ChatId, result.Value.InitialAssistantReply, result.Value.DiagnosticResultJson, result.Value.WorkClarificationResultJson, result.Value.PartnerAdviceResultJson));
    }

    private static async Task<IResult> GetChatMessages(
        Guid chatId,
        ISender mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetChatMessagesQuery(chatId), ct);

        if (result.IsFailure)
            return Results.NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = result.Error!.Code,
                Detail = result.Error.Description
            });

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> SendMessage(
        Guid chatId,
        [FromBody] SendMessageRequest request,
        ISender mediator,
        CancellationToken ct)
    {
        var command = new SendMessageCommand(
            ChatId: chatId,
            Content: request.Content,
            Locale: request.Locale ?? "ru");

        var result = await mediator.Send(command, ct);

        if (result.IsFailure)
        {
            var errorCode = result.Error!.Code;

            if (errorCode == AppErrors.Chat.NotFound.Code)
                return Results.NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = result.Error.Code,
                    Detail = result.Error.Description
                });

            if (errorCode == AppErrors.Auth.NotAuthenticated.Code)
                return Results.Unauthorized();

            if (errorCode == AppErrors.Chat.ChatIsCompleted.Code)
                return Results.Problem(
                    statusCode: StatusCodes.Status409Conflict,
                    title: result.Error.Code,
                    detail: result.Error.Description);

            return Results.Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: result.Error.Code,
                detail: result.Error.Description);
        }

        return Results.Ok(new SendMessageApiResponse(
            result.Value.AssistantReply,
            result.Value.WasValid,
            result.Value.ResponseStage,
            result.Value.ChatStatus.ToString(),
            result.Value.DiagnosticResultJson));
    }

    private static async Task<IResult> DeleteChat(
        Guid chatId,
        ISender mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteChatCommand(chatId), ct);

        if (result.IsFailure)
        {
            if (result.Error!.Code == AppErrors.Auth.NotAuthenticated.Code)
                return Results.Unauthorized();

            return Results.NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = result.Error.Code,
                Detail = result.Error.Description
            });
        }

        return Results.NoContent();
    }

    // ─── Request / Response DTOs ──────────────────────────────────────────────

    private sealed record DiagnosticsInputRequest(
        string Symptoms,
        string? RecentEvents,
        string? PreviousIssues);

    private sealed record WorkClarificationInputRequest(
        string WorksPerformed,
        string WorkReason,
        decimal LaborCost,
        decimal PartsCost,
        string? Guarantees);

    private sealed record PartnerAdviceInputRequest(
        string Request,
        double Lat,
        double Lng,
        string? Urgency);

    private sealed record CreateChatApiRequest(
        ChatMode Mode,
        string Title,
        Guid? VehicleId,
        DiagnosticsInputRequest? DiagnosticsInput,
        WorkClarificationInputRequest? WorkClarificationInput,
        PartnerAdviceInputRequest? PartnerAdviceInput,
        string? Locale);

    private sealed record CreateChatApiResponse(Guid ChatId, string? InitialAssistantReply, string? DiagnosticResultJson, string? WorkClarificationResultJson, string? PartnerAdviceResultJson);

    private sealed record SendMessageRequest(string Content, string? Locale);

    private sealed record SendMessageApiResponse(
        string AssistantReply,
        bool WasValid,
        string? ResponseStage,
        string ChatStatus,
        string? DiagnosticResultJson);
}
