using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.Partners.RegisterPartner;

/// <summary>
/// Registers a new partner profile for the currently authenticated user.
/// The partner will be inactive until verified by an administrator.
/// </summary>
public sealed record RegisterPartnerCommand(
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
    string? ContactsWebsite = null,
    string? ContactsMessengerLinks = null) : IRequest<Result<Guid>>;
