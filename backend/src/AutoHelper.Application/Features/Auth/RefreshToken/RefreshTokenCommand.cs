using AutoHelper.Application.Common;
using AutoHelper.Application.Features.Auth.Login;
using MediatR;

namespace AutoHelper.Application.Features.Auth.RefreshToken;

/// <summary>
/// Exchanges a valid refresh token for a new pair of access and refresh tokens.
/// The old refresh token is revoked (token rotation).
/// </summary>
public sealed record RefreshTokenCommand(string Token) : IRequest<Result<TokenResponse>>;
