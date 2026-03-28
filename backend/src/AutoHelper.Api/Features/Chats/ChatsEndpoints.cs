using AutoHelper.Application.Features.Chats;
using AutoHelper.Application.Features.Chats.CreateChat;
using AutoHelper.Application.Features.Chats.GetChatMessages;
using AutoHelper.Application.Features.Chats.GetMyChats;
using AutoHelper.Application.Features.Chats.SendMessage;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AutoHelper.Api.Features.Chats;

public static class ChatsEndpoints
{
    public static void MapChatEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/chats").WithTags("Chats").RequireAuthorization();

        group.MapGet("/", GetMyChats)
            .WithSummary("Get all chat sessions for the authenticated customer");

        group.MapPost("/", CreateChat)
            .WithSummary("Create a new AI chat session (optionally linked to a vehicle)")
            .Produces<CreateChatResponse>(StatusCodes.Status201Created)
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
    }

    // ─── Handlers ─────────────────────────────────────────────────────────────

    private static async Task<IResult> GetMyChats(
        ISender mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetMyChatsQuery(), ct);

        if (result.IsFailure)
            return Results.Unauthorized();

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> CreateChat(
        [FromBody] CreateChatCommand command,
        ISender mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);

        if (result.IsFailure)
        {
            if (result.Error == ChatErrors.NotAuthenticated)
                return Results.Unauthorized();

            // Premium subscription required
            return Results.Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: result.Error);
        }

        return Results.Created($"/api/chats/{result.Value}", new CreateChatResponse(result.Value));
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
                Title = result.Error
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
            if (result.Error == ChatErrors.ChatNotFound)
                return Results.NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = result.Error
                });

            if (result.Error == ChatErrors.NotAuthenticated)
                return Results.Unauthorized();

            // Premium subscription required
            return Results.Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: result.Error);
        }

        return Results.Ok(new SendMessageApiResponse(
            result.Value.AssistantReply,
            result.Value.WasValid));
    }

    // ─── Response / Request DTOs ──────────────────────────────────────────────

    private sealed record CreateChatResponse(Guid ChatId);

    private sealed record SendMessageRequest(string Content, string? Locale);

    private sealed record SendMessageApiResponse(string AssistantReply, bool WasValid);
}
