using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.Clients.UpdateMyProfile;

/// <summary>
/// Updates the display name and contact information of the currently authenticated customer.
/// </summary>
public sealed record UpdateMyProfileCommand(
    string Name,
    string? Contacts) : IRequest<Result>;
