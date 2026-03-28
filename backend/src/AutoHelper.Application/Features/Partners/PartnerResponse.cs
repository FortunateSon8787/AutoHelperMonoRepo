using AutoHelper.Domain.Partners;

namespace AutoHelper.Application.Features.Partners;

/// <summary>
/// Public-facing DTO for partner profile data.
/// </summary>
public sealed record PartnerResponse(
    Guid Id,
    string Name,
    string Type,
    string Specialization,
    string Description,
    string Address,
    double LocationLat,
    double LocationLng,
    string WorkingOpenFrom,
    string WorkingOpenTo,
    string WorkingDays,
    string ContactsPhone,
    string? ContactsWebsite,
    string? ContactsMessengerLinks,
    string? LogoUrl,
    bool IsVerified,
    bool IsActive,
    bool ShowBannersToAnonymous,
    Guid AccountUserId)
{
    public static PartnerResponse FromPartner(Partner p) => new(
        Id: p.Id,
        Name: p.Name,
        Type: p.Type.ToString(),
        Specialization: p.Specialization,
        Description: p.Description,
        Address: p.Address,
        LocationLat: p.Location.Lat,
        LocationLng: p.Location.Lng,
        WorkingOpenFrom: p.WorkingHours.OpenFrom.ToString("HH:mm"),
        WorkingOpenTo: p.WorkingHours.OpenTo.ToString("HH:mm"),
        WorkingDays: p.WorkingHours.WorkDays,
        ContactsPhone: p.Contacts.Phone,
        ContactsWebsite: p.Contacts.Website,
        ContactsMessengerLinks: p.Contacts.MessengerLinks,
        LogoUrl: p.LogoUrl,
        IsVerified: p.IsVerified,
        IsActive: p.IsActive,
        ShowBannersToAnonymous: p.ShowBannersToAnonymous,
        AccountUserId: p.AccountUserId);
}
