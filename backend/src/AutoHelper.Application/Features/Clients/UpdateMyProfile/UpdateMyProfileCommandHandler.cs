using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using MediatR;

namespace AutoHelper.Application.Features.Clients.UpdateMyProfile;

public sealed class UpdateMyProfileCommandHandler(
    ICustomerRepository customers,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateMyProfileCommand, Result>
{
    public async Task<Result> Handle(UpdateMyProfileCommand request, CancellationToken ct)
    {
        if (currentUser.Id is null)
            return AppErrors.Auth.NotAuthenticated;

        var customer = await customers.GetByIdAsync(currentUser.Id.Value, ct);
        if (customer is null)
            return AppErrors.Customer.NotFound;

        customer.UpdateProfile(request.Name, request.Contacts);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
