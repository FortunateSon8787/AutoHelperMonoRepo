using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using AutoHelper.Domain.Customers;
using MediatR;

namespace AutoHelper.Application.Features.Auth.Register;

public sealed class RegisterCustomerCommandHandler(
    ICustomerRepository customers,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork) : IRequestHandler<RegisterCustomerCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(RegisterCustomerCommand request, CancellationToken ct)
    {
        var emailExists = await customers.ExistsByEmailAsync(request.Email, ct);
        if (emailExists)
            return AppErrors.Auth.EmailAlreadyExists;

        var passwordHash = passwordHasher.Hash(request.Password);

        var customer = Customer.CreateWithPassword(
            name: request.Name,
            email: request.Email,
            passwordHash: passwordHash,
            contacts: request.Contacts);

        customers.Add(customer);
        await unitOfWork.SaveChangesAsync(ct);

        return Result<Guid>.Success(customer.Id);
    }
}
