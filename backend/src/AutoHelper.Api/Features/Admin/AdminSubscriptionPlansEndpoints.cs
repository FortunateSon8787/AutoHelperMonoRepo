using AutoHelper.Application.Features.Admin.SubscriptionPlans.GetAllPlanConfigs;
using AutoHelper.Application.Features.Admin.SubscriptionPlans.UpdatePlanConfig;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AutoHelper.Api.Features.Admin;

public static class AdminSubscriptionPlansEndpoints
{
    public static void MapAdminSubscriptionPlansEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/subscription-plans")
            .WithTags("Admin — Subscription Plans")
            .RequireAuthorization("admin");

        group.MapGet("/", GetAll)
            .WithSummary("Get all subscription plan configurations")
            .Produces<IReadOnlyList<PlanConfigResponse>>(StatusCodes.Status200OK);

        group.MapPut("/{plan}", Update)
            .WithSummary("Update price and monthly quota for a subscription plan")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> GetAll(ISender mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new GetAllPlanConfigsQuery(), ct);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.Problem(title: result.Error!.Code, detail: result.Error.Description,
                statusCode: StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> Update(
        string plan,
        [FromBody] UpdatePlanConfigRequest body,
        ISender mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new UpdatePlanConfigCommand(plan, body.PriceUsd, body.MonthlyQuota), ct);
        return result.IsSuccess
            ? Results.NoContent()
            : Results.Problem(title: result.Error!.Code, detail: result.Error.Description,
                statusCode: StatusCodes.Status400BadRequest);
    }
}

public sealed record UpdatePlanConfigRequest(decimal PriceUsd, int MonthlyQuota);
