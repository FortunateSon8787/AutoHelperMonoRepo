using AutoHelper.Application.Features.Admin.AdCampaigns;
using AutoHelper.Application.Features.Admin.AdCampaigns.ActivateAdCampaign;
using AutoHelper.Application.Features.Admin.AdCampaigns.DeactivateAdCampaign;
using AutoHelper.Application.Features.Admin.AdCampaigns.GetAdminAdCampaigns;
using MediatR;

namespace AutoHelper.Api.Features.Admin;

public static class AdminAdCampaignsEndpoints
{
    public static void MapAdminAdCampaignsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/ad-campaigns")
            .WithTags("Admin — Ad Campaigns")
            .RequireAuthorization("admin");

        group.MapGet("/", GetAll)
            .WithSummary("Get paginated list of all ad campaigns with optional filter by partner")
            .Produces<AdminAdCampaignListResponse>(StatusCodes.Status200OK);

        group.MapPost("/{id:guid}/activate", Activate)
            .WithSummary("Activate an ad campaign")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/deactivate", Deactivate)
            .WithSummary("Deactivate an ad campaign")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetAll(
        ISender mediator,
        CancellationToken ct,
        int page = 1,
        int pageSize = 20,
        Guid? partnerId = null)
    {
        var result = await mediator.Send(new GetAdminAdCampaignsQuery(page, pageSize, partnerId), ct);
        return Results.Ok(result.Value);
    }

    private static async Task<IResult> Activate(Guid id, ISender mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new ActivateAdCampaignCommand(id), ct);
        return result.IsSuccess
            ? Results.NoContent()
            : Results.Problem(
                title: result.Error!.Code,
                detail: result.Error.Description,
                statusCode: result.Error.Code == "ADMIN_009"
                    ? StatusCodes.Status404NotFound
                    : StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> Deactivate(Guid id, ISender mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new DeactivateAdCampaignCommand(id), ct);
        return result.IsSuccess
            ? Results.NoContent()
            : Results.Problem(
                title: result.Error!.Code,
                detail: result.Error.Description,
                statusCode: result.Error.Code == "ADMIN_009"
                    ? StatusCodes.Status404NotFound
                    : StatusCodes.Status400BadRequest);
    }
}
