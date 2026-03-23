using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.Auth.Login;

/// <summary>
/// Authenticates a customer with email and password,
/// returning a JWT access token and an opaque refresh token.
/// </summary>
public sealed record LoginCommand(string Email, string Password) : IRequest<Result<TokenResponse>>;
