using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using MediatR;

namespace AutoHelper.Application.Features.Admin.Customers.GetAdminCustomerById;

public sealed class GetAdminCustomerByIdQueryHandler(
    ICustomerRepository customers,
    IInvalidChatRequestRepository invalidChatRequests)
    : IRequestHandler<GetAdminCustomerByIdQuery, Result<AdminCustomerResponse>>
{
    public async Task<Result<AdminCustomerResponse>> Handle(
        GetAdminCustomerByIdQuery request, CancellationToken ct)
    {
        var customer = await customers.GetByIdAsync(request.CustomerId, ct);
        if (customer is null)
            return Result<AdminCustomerResponse>.Failure(AppErrors.Admin.CustomerNotFound);

        var invalidCount = await invalidChatRequests.CountByCustomerAsync(customer.Id, ct);
        return Result<AdminCustomerResponse>.Success(
            AdminCustomerResponse.FromCustomer(customer, invalidCount));
    }
}
