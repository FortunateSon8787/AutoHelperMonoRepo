using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Domain.Customers;
using MediatR;

namespace AutoHelper.Application.Features.Clients.ActivateSubscription;

public sealed class ActivateSubscriptionCommandHandler(
    ICustomerRepository customers,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork) : IRequestHandler<ActivateSubscriptionCommand, Result>
{
    public async Task<Result> Handle(ActivateSubscriptionCommand request, CancellationToken ct)
    {
        if (currentUser.Id is null)
            return AppErrors.Auth.NotAuthenticated;

        if (!Enum.TryParse<SubscriptionPlan>(request.Plan, ignoreCase: true, out var plan)
            || plan == SubscriptionPlan.None)
            return AppErrors.Subscription.InvalidPlan;

        var customer = await customers.GetByIdAsync(currentUser.Id.Value, ct);
        if (customer is null)
            return AppErrors.Customer.NotFound;

        customer.ActivateSubscription(plan);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
