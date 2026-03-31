using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using MediatR;

namespace AutoHelper.Application.Features.Admin.Customers.UnblockCustomer;

public sealed class UnblockCustomerCommandHandler(
    ICustomerRepository customers,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UnblockCustomerCommand, Result>
{
    public async Task<Result> Handle(UnblockCustomerCommand request, CancellationToken ct)
    {
        var customer = await customers.GetByIdAsync(request.CustomerId, ct);
        if (customer is null)
            return Result.Failure(AppErrors.Admin.CustomerNotFound);

        if (!customer.IsBlocked)
            return Result.Failure(AppErrors.Admin.CustomerNotBlocked);

        customer.Unblock();
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
