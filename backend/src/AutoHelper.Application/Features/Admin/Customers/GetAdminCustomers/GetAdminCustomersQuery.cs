using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.Admin.Customers.GetAdminCustomers;

public sealed record GetAdminCustomersQuery(
    int Page,
    int PageSize,
    string? Search) : IRequest<Result<AdminCustomerListResponse>>;
