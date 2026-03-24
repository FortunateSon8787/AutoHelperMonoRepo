using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using MediatR;

namespace AutoHelper.Application.Features.Clients.GetMyProfile;

public sealed class GetMyProfileQueryHandler(
    ICustomerRepository customers,
    ICurrentUser currentUser) : IRequestHandler<GetMyProfileQuery, Result<ClientProfileResponse>>
{
    public async Task<Result<ClientProfileResponse>> Handle(GetMyProfileQuery request, CancellationToken ct)
    {
        if (currentUser.Id is null)
            return Result<ClientProfileResponse>.Failure("User is not authenticated.");

        var customer = await customers.GetByIdAsync(currentUser.Id.Value, ct);
        if (customer is null)
            return Result<ClientProfileResponse>.Failure("Customer not found.");

        return Result<ClientProfileResponse>.Success(ClientProfileResponse.FromCustomer(customer));
    }
}
