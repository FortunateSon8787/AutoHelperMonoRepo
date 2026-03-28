using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.Partners.UpdateMyPartnerProfile;

/// <summary>Updates the profile of the currently authenticated partner.</summary>
public sealed record UpdateMyPartnerProfileCommand(
    string Name,
    string Specialization,
    string Description,
    string Address,
    double LocationLat,
    double LocationLng,
    string WorkingOpenFrom,
    string WorkingOpenTo,
    string WorkingDays,
    string ContactsPhone,
    string? ContactsWebsite = null,
    string? ContactsMessengerLinks = null) : IRequest<Result>;
