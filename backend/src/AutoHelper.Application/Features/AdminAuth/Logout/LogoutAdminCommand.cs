using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.AdminAuth.Logout;

/// <summary>
/// Revokes the admin refresh token identified by the given token value.
/// </summary>
public sealed record LogoutAdminCommand(string RefreshToken) : IRequest<Result>;
