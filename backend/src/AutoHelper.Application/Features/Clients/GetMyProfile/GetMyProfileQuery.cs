using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.Clients.GetMyProfile;

/// <summary>
/// Returns the profile of the currently authenticated customer.
/// </summary>
public sealed record GetMyProfileQuery : IRequest<Result<ClientProfileResponse>>;
