using AutoHelper.Domain.Customers;

namespace AutoHelper.Application.Features.Vehicles.GetVehicleOwner;

/// <summary>
/// Public profile of the vehicle owner returned when looking up by VIN.
/// </summary>
public sealed record VehicleOwnerResponse(
    Guid OwnerId,
    string Name,
    string? Contacts,
    string? AvatarUrl)
{
    public static VehicleOwnerResponse FromCustomer(Customer customer) => new(
        OwnerId: customer.Id,
        Name: customer.Name,
        Contacts: customer.Contacts,
        AvatarUrl: customer.AvatarUrl);
}
