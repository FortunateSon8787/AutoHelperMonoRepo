using AutoHelper.Api.Extensions;
using AutoHelper.Application.Features.Admin.Customers;
using AutoHelper.Application.Features.Admin.Customers.BlockCustomer;
using AutoHelper.Application.Features.Admin.Customers.GetAdminCustomerById;
using AutoHelper.Application.Features.Admin.Customers.GetAdminCustomers;
using AutoHelper.Application.Features.Admin.Customers.UnblockCustomer;
using MediatR;

namespace AutoHelper.Api.Features.Admin;

public static class AdminCustomersEndpoints
{
    public static void MapAdminCustomersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/customers")
            .WithTags("Admin — Customers")
            .RequireAuthorization(WebApplicationBuilderExtensions.AdminPolicy);

        group.MapGet("/", GetAll)
            .WithSummary("Get paginated list of customers with optional search by name or email")
            .Produces<AdminCustomerListResponse>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", GetById)
            .WithSummary("Get a single customer by ID")
            .Produces<AdminCustomerResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/block", Block)
            .WithSummary("Block a customer account")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/unblock", Unblock)
            .WithSummary("Unblock a customer account")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetAll(
        ISender mediator,
        CancellationToken ct,
        int page = 1,
        int pageSize = 20,
        string? search = null)
    {
        var result = await mediator.Send(new GetAdminCustomersQuery(page, pageSize, search), ct);
        return Results.Ok(result.Value);
    }

    private static async Task<IResult> GetById(Guid id, ISender mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new GetAdminCustomerByIdQuery(id), ct);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.Problem(title: result.Error!.Code, detail: result.Error.Description,
                statusCode: StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Block(Guid id, ISender mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new BlockCustomerCommand(id), ct);
        return result.IsSuccess
            ? Results.NoContent()
            : Results.Problem(title: result.Error!.Code, detail: result.Error.Description,
                statusCode: result.Error.ToHttpStatusCode());
    }

    private static async Task<IResult> Unblock(Guid id, ISender mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new UnblockCustomerCommand(id), ct);
        return result.IsSuccess
            ? Results.NoContent()
            : Results.Problem(title: result.Error!.Code, detail: result.Error.Description,
                statusCode: result.Error.ToHttpStatusCode());
    }
}
