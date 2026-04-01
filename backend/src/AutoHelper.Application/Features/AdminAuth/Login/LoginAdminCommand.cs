using AutoHelper.Application.Common;
using AutoHelper.Application.Features.Auth.Login;
using MediatR;

namespace AutoHelper.Application.Features.AdminAuth.Login;

/// <summary>
/// Authenticates an admin user with email and password,
/// returning a JWT access token and an opaque refresh token.
/// </summary>
public sealed record LoginAdminCommand(string Email, string Password) : IRequest<Result<TokenResponse>>;
