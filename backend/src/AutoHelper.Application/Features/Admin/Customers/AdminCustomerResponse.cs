using AutoHelper.Domain.Customers;

namespace AutoHelper.Application.Features.Admin.Customers;

public sealed record AdminCustomerResponse(
    Guid Id,
    string Name,
    string Email,
    string? Contacts,
    string SubscriptionStatus,
    string SubscriptionPlan,
    int AiRequestsRemaining,
    string AuthProvider,
    DateTime RegistrationDate,
    bool IsBlocked,
    int InvalidChatRequestCount)
{
    public static AdminCustomerResponse FromCustomer(Customer customer, int invalidChatRequestCount) => new(
        Id: customer.Id,
        Name: customer.Name,
        Email: customer.Email,
        Contacts: customer.Contacts,
        SubscriptionStatus: customer.SubscriptionStatus.ToString(),
        SubscriptionPlan: customer.SubscriptionPlan.ToString(),
        AiRequestsRemaining: customer.AiRequestsRemaining,
        AuthProvider: customer.AuthProvider.ToString(),
        RegistrationDate: customer.RegistrationDate,
        IsBlocked: customer.IsBlocked,
        InvalidChatRequestCount: invalidChatRequestCount);
}

public sealed record AdminCustomerListResponse(
    IReadOnlyList<AdminCustomerResponse> Items,
    int TotalCount,
    int Page,
    int PageSize);
