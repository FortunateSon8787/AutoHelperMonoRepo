using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using MediatR;

namespace AutoHelper.Application.Features.Admin.Customers.BlockCustomer;

public sealed class BlockCustomerCommandHandler(
    ICustomerRepository customers,
    IUnitOfWork unitOfWork)
    : IRequestHandler<BlockCustomerCommand, Result>
{
    public async Task<Result> Handle(BlockCustomerCommand request, CancellationToken ct)
    {
        var customer = await customers.GetByIdAsync(request.CustomerId, ct);
        if (customer is null)
            return Result.Failure(AppErrors.Admin.CustomerNotFound);

        if (customer.IsBlocked)
            return Result.Failure(AppErrors.Admin.CustomerAlreadyBlocked);

        customer.Block();
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
