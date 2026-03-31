using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using MediatR;

namespace AutoHelper.Application.Features.Clients.GetMySubscription;

public sealed class GetMySubscriptionQueryHandler(
    ICustomerRepository customers,
    ISubscriptionPlanConfigRepository planConfigs,
    ICurrentUser currentUser) : IRequestHandler<GetMySubscriptionQuery, Result<SubscriptionResponse>>
{
    public async Task<Result<SubscriptionResponse>> Handle(GetMySubscriptionQuery request, CancellationToken ct)
    {
        if (currentUser.Id is null)
            return AppErrors.Auth.NotAuthenticated;

        var customer = await customers.GetByIdAsync(currentUser.Id.Value, ct);
        if (customer is null)
            return AppErrors.Customer.NotFound;

        var config = customer.SubscriptionPlan != Domain.Customers.SubscriptionPlan.None
            ? await planConfigs.GetByPlanAsync(customer.SubscriptionPlan, ct)
            : null;

        return Result<SubscriptionResponse>.Success(SubscriptionResponse.FromCustomer(customer, config));
    }
}
