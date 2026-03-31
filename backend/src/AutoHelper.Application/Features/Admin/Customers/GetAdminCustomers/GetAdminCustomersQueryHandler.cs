using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using MediatR;

namespace AutoHelper.Application.Features.Admin.Customers.GetAdminCustomers;

public sealed class GetAdminCustomersQueryHandler(
    ICustomerRepository customers,
    IInvalidChatRequestRepository invalidChatRequests)
    : IRequestHandler<GetAdminCustomersQuery, Result<AdminCustomerListResponse>>
{
    public async Task<Result<AdminCustomerListResponse>> Handle(
        GetAdminCustomersQuery request, CancellationToken ct)
    {
        var (items, totalCount) = await customers.GetPagedAsync(
            request.Page, request.PageSize, request.Search, ct);

        var responseItems = new List<AdminCustomerResponse>(items.Count);
        foreach (var customer in items)
        {
            var invalidCount = await invalidChatRequests.CountByCustomerAsync(customer.Id, ct);
            responseItems.Add(AdminCustomerResponse.FromCustomer(customer, invalidCount));
        }

        return Result<AdminCustomerListResponse>.Success(
            new AdminCustomerListResponse(responseItems, totalCount, request.Page, request.PageSize));
    }
}
