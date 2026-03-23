using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.Auth.Logout;

/// <summary>
/// Invalidates the given refresh token so it can no longer be used.
/// Effectively logs out the session associated with this token.
/// </summary>
public sealed record LogoutCommand(string RefreshToken) : IRequest<Result>;
