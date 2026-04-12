using AutoHelper.Api.Extensions;
using AutoHelper.Application.Features.Admin.Vehicles;
using AutoHelper.Application.Features.Admin.Vehicles.GetAdminVehicleById;
using AutoHelper.Application.Features.Admin.Vehicles.GetAdminVehicles;
using MediatR;

namespace AutoHelper.Api.Features.Admin;

public static class AdminVehiclesEndpoints
{
    public static void MapAdminVehiclesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/vehicles")
            .WithTags("Admin — Vehicles")
            .RequireAuthorization(WebApplicationBuilderExtensions.AdminPolicy);

        group.MapGet("/", GetAll)
            .WithSummary("Get paginated list of vehicles with optional search by VIN, brand or model")
            .Produces<AdminVehicleListResponse>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", GetById)
            .WithSummary("Get a single vehicle by ID")
            .Produces<AdminVehicleResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetAll(
        ISender mediator,
        CancellationToken ct,
        int page = 1,
        int pageSize = 20,
        string? search = null)
    {
        var result = await mediator.Send(new GetAdminVehiclesQuery(page, pageSize, search), ct);
        return Results.Ok(result.Value);
    }

    private static async Task<IResult> GetById(Guid id, ISender mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new GetAdminVehicleByIdQuery(id), ct);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.Problem(title: result.Error!.Code, detail: result.Error.Description,
                statusCode: StatusCodes.Status404NotFound);
    }
}
