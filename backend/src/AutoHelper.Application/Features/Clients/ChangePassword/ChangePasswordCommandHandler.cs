using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using MediatR;

namespace AutoHelper.Application.Features.Clients.ChangePassword;

public sealed class ChangePasswordCommandHandler(
    ICustomerRepository customers,
    IPasswordHasher passwordHasher,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork) : IRequestHandler<ChangePasswordCommand, Result>
{
    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken ct)
    {
        if (currentUser.Id is null)
            return AppErrors.Auth.NotAuthenticated;

        var customer = await customers.GetByIdAsync(currentUser.Id.Value, ct);
        if (customer is null)
            return AppErrors.Customer.NotFound;

        if (customer.PasswordHash is null)
            return AppErrors.Customer.PasswordChangeNotAvailableForOAuth;

        if (!passwordHasher.Verify(request.CurrentPassword, customer.PasswordHash))
            return AppErrors.Customer.IncorrectCurrentPassword;

        var newHash = passwordHasher.Hash(request.NewPassword);
        customer.ChangePassword(newHash);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
