using AutoHelper.Application.Features.Vehicles.GetVehicleOwner;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AutoHelper.Api.Features.Vehicles;

public static class VehiclesEndpoints
{
    public static void MapVehicleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/vehicles").WithTags("Vehicles");

        group.MapGet("/{vin}/owner", GetVehicleOwner)
            .WithSummary("Get the public profile of the owner of a vehicle identified by VIN")
            .Produces<VehicleOwnerResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);
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
}
