using AutoHelper.Application.Common;
using AutoHelper.Application.Features.Auth.Login;
using MediatR;

namespace AutoHelper.Application.Features.AdminAuth.RefreshToken;

/// <summary>
/// Exchanges a valid admin refresh token for a new pair of access and refresh tokens.
/// The old refresh token is revoked (token rotation).
/// </summary>
public sealed record RefreshAdminTokenCommand(string Token) : IRequest<Result<TokenResponse>>;
