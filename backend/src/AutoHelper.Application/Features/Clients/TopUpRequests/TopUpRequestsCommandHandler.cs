using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using MediatR;

namespace AutoHelper.Application.Features.Clients.TopUpRequests;

public sealed class TopUpRequestsCommandHandler(
    ICustomerRepository customers,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork) : IRequestHandler<TopUpRequestsCommand, Result>
{
    public async Task<Result> Handle(TopUpRequestsCommand request, CancellationToken ct)
    {
        if (currentUser.Id is null)
            return AppErrors.Auth.NotAuthenticated;

        if (request.Count <= 0)
            return AppErrors.Subscription.InsufficientRequestsForTopUp;

        var customer = await customers.GetByIdAsync(currentUser.Id.Value, ct);
        if (customer is null)
            return AppErrors.Customer.NotFound;

        customer.TopUpRequests(request.Count);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
