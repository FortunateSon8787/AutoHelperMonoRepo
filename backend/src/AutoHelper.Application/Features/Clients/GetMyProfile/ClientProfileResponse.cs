using AutoHelper.Domain.Customers;

namespace AutoHelper.Application.Features.Clients.GetMyProfile;

public sealed record ClientProfileResponse(
    Guid Id,
    string Name,
    string Email,
    string? Contacts,
    string SubscriptionStatus,
    string AuthProvider,
    DateTime RegistrationDate)
{
    public static ClientProfileResponse FromCustomer(Customer customer) => new(
        Id: customer.Id,
        Name: customer.Name,
        Email: customer.Email,
        Contacts: customer.Contacts,
        SubscriptionStatus: customer.SubscriptionStatus.ToString(),
        AuthProvider: customer.AuthProvider.ToString(),
        RegistrationDate: customer.RegistrationDate);
}
