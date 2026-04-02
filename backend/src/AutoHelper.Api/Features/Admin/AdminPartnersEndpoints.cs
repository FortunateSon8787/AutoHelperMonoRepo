using AutoHelper.Application.Features.Admin.Partners;
using AutoHelper.Application.Features.Admin.Partners.DeactivatePartner;
using AutoHelper.Application.Features.Admin.Partners.DeleteAdminReview;
using AutoHelper.Application.Features.Admin.Partners.DeletePartner;
using AutoHelper.Application.Features.Admin.Partners.GetAdminPartnerById;
using AutoHelper.Application.Features.Admin.Partners.GetAdminPartners;
using AutoHelper.Application.Features.Admin.Partners.GetPotentiallyUnfitPartners;
using AutoHelper.Application.Features.Admin.Partners.VerifyPartner;
using MediatR;

namespace AutoHelper.Api.Features.Admin;

public static class AdminPartnersEndpoints
{
    public static void MapAdminPartnersEndpoints(this IEndpointRouteBuilder app)
    {
        var partnersGroup = app.MapGroup("/api/admin/partners")
            .WithTags("Admin — Partners")
            .RequireAuthorization("admin");

        partnersGroup.MapGet("/", GetAll)
            .WithSummary("Get paginated list of partners with optional search by name or address")
            .Produces<AdminPartnerListResponse>(StatusCodes.Status200OK);

        partnersGroup.MapGet("/unfit", GetUnfit)
            .WithSummary("Get partners flagged as potentially unfit with their low-rating reviews")
            .Produces<IReadOnlyList<AdminUnfitPartnerResponse>>(StatusCodes.Status200OK);

        partnersGroup.MapGet("/{id:guid}", GetById)
            .WithSummary("Get a single partner by ID with their reviews")
            .Produces<AdminPartnerDetailResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        partnersGroup.MapPost("/{id:guid}/verify", Verify)
            .WithSummary("Verify a partner and activate their profile")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        partnersGroup.MapPost("/{id:guid}/deactivate", Deactivate)
            .WithSummary("Deactivate a partner and disable all their active ad campaigns")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        partnersGroup.MapDelete("/{id:guid}", Delete)
            .WithSummary("Soft-delete a partner")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        var reviewsGroup = app.MapGroup("/api/admin/reviews")
            .WithTags("Admin — Partners")
            .RequireAuthorization("admin");

        reviewsGroup.MapDelete("/{id:guid}", DeleteReview)
            .WithSummary("Soft-delete a partner review")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetAll(
        ISender mediator,
        CancellationToken ct,
        int page = 1,
        int pageSize = 20,
        string? search = null)
    {
        var result = await mediator.Send(new GetAdminPartnersQuery(page, pageSize, search), ct);
        return Results.Ok(result.Value);
    }

    private static async Task<IResult> GetUnfit(ISender mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new GetPotentiallyUnfitPartnersQuery(), ct);
        return Results.Ok(result.Value);
    }

    private static async Task<IResult> GetById(Guid id, ISender mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new GetAdminPartnerByIdQuery(id), ct);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.Problem(title: result.Error!.Code, detail: result.Error.Description,
                statusCode: StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Verify(Guid id, ISender mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new VerifyPartnerCommand(id), ct);
        return result.IsSuccess
            ? Results.NoContent()
            : Results.Problem(title: result.Error!.Code, detail: result.Error.Description,
                statusCode: result.Error.ToHttpStatusCode());
    }

    private static async Task<IResult> Deactivate(Guid id, ISender mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new DeactivatePartnerCommand(id), ct);
        return result.IsSuccess
            ? Results.NoContent()
            : Results.Problem(title: result.Error!.Code, detail: result.Error.Description,
                statusCode: result.Error.ToHttpStatusCode());
    }

    private static async Task<IResult> Delete(Guid id, ISender mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new DeletePartnerCommand(id), ct);
        return result.IsSuccess
            ? Results.NoContent()
            : Results.Problem(title: result.Error!.Code, detail: result.Error.Description,
                statusCode: StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> DeleteReview(Guid id, ISender mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteAdminReviewCommand(id), ct);
        return result.IsSuccess
            ? Results.NoContent()
            : Results.Problem(title: result.Error!.Code, detail: result.Error.Description,
                statusCode: StatusCodes.Status404NotFound);
    }
}
