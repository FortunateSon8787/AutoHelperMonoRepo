using AutoHelper.Api.Extensions;
using AutoHelper.Application.Features.Admin.ChatbotConfig.GetChatbotConfig;
using AutoHelper.Application.Features.Admin.ChatbotConfig.UpdateChatbotConfig;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AutoHelper.Api.Features.Admin;

public static class AdminChatbotConfigEndpoints
{
    public static void MapAdminChatbotConfigEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/chatbot-config")
            .WithTags("Admin — Chatbot Config")
            .RequireAuthorization(WebApplicationBuilderExtensions.AdminPolicy);

        group.MapGet("/", Get)
            .WithSummary("Get current chatbot configuration")
            .Produces<Application.Features.Admin.ChatbotConfig.ChatbotConfigResponse>(StatusCodes.Status200OK);

        group.MapPut("/", Update)
            .WithSummary("Update chatbot configuration")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> Get(ISender mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new GetChatbotConfigQuery(), ct);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.Problem(title: result.Error!.Code, detail: result.Error.Description,
                statusCode: StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> Update(
        [FromBody] UpdateChatbotConfigRequest body,
        ISender mediator,
        CancellationToken ct)
    {
        var command = new UpdateChatbotConfigCommand(
            body.IsEnabled,
            body.MaxCharsPerField,
            body.DailyLimitByPlan,
            body.TopUpPriceUsd,
            body.TopUpRequestCount,
            body.DisablePartnerSuggestionsInMode1);

        var result = await mediator.Send(command, ct);
        return result.IsSuccess
            ? Results.NoContent()
            : Results.Problem(title: result.Error!.Code, detail: result.Error.Description,
                statusCode: StatusCodes.Status400BadRequest);
    }
}

public sealed record UpdateChatbotConfigRequest(
    bool IsEnabled,
    int MaxCharsPerField,
    Dictionary<string, int> DailyLimitByPlan,
    decimal TopUpPriceUsd,
    int TopUpRequestCount,
    bool DisablePartnerSuggestionsInMode1);
